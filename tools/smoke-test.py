#!/usr/bin/env python3
"""Exercise authenticated sales, inventory, paginated lookups and invoice PDFs."""
import argparse
import json
import uuid
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path
from urllib.request import Request, urlopen
from urllib.error import HTTPError

parser = argparse.ArgumentParser()
parser.add_argument('--api', default='http://localhost:5100')
parser.add_argument('--require-linux', action='store_true')
parser.add_argument('--output', default='artifacts/Net10')
args = parser.parse_args()
base = args.api.rstrip('/')
checks = 0

def request(path, body=None, token=None):
    headers = {'Content-Type': 'application/json'}
    if token: headers['Authorization'] = 'Bearer ' + token
    data = None if body is None else json.dumps(body).encode()
    try:
        response = urlopen(Request(base + path, data=data, headers=headers), timeout=30)
    except HTTPError as error:
        response = error
    raw = response.read()
    content = response.headers.get('Content-Type', '')
    result = json.loads(raw) if 'json' in content else raw
    return response.code, result

def expect(response, status, label):
    global checks
    assert response[0] == status, f'{label}: expected {status}, got {response[0]}: {response[1]}'
    checks += 1
    print(f'OK {label} ({status})')
    return response[1]

def verify(condition, label):
    global checks
    assert condition, label
    checks += 1
    print('OK ' + label)

health = expect(request('/health'), 200, 'Health')
verify(health['framework'] == 'net10.0', '.NET 10 target')
if args.require_linux: verify('linux' in health['os'].lower(), 'Backend running on Linux')
email = 'smoke-' + uuid.uuid4().hex[:10] + '@example.test'
registration = {'email': email, 'password': 'Test123!Safe', 'firstName': 'Smoke', 'lastName': 'Test'}
expect(request('/user/Register', registration), 200, 'Registration')
expect(request('/user/Register', registration), 400, 'Duplicate email rejected')
expect(request('/user/Login', {'email': email, 'password': 'Incorrect123!'}), 401, 'Incorrect credentials rejected')
token = expect(request('/user/Login', {'email': email, 'password': registration['password']}), 200, 'Login')['accessToken']
expect(request('/catalog/products'), 401, 'Anonymous catalog rejected')
products = expect(request('/catalog/products', token=token), 200, 'Available products')
verify(all(p['unitsInStock'] > 0 for p in products), 'Out-of-stock products hidden')
product = max(products, key=lambda p: p['unitsInStock'])
customers = expect(request('/catalog/customers/search?q=alfki&page=1&pageSize=1', token=token), 200, 'Customer search by ID')
verify(customers['items'][0]['id'] == 'ALFKI', 'Case-insensitive customer search')
from urllib.parse import quote
expect(request('/catalog/customers/search?q=alfreds&page=1&pageSize=1', token=token), 200, 'Customer search by name')
found = expect(request('/catalog/products/search?q=' + quote(product['name']) + '&page=1&pageSize=1', token=token), 200, 'Product search by name')
verify(any(p['id'] == product['id'] for p in found['items']), 'Product lookup matches selected product')
expect(request('/catalog/products/search?q=' + str(product['id']) + '&page=1&pageSize=1', token=token), 200, 'Product search by ID')
first = expect(request('/catalog/products/search?page=1&pageSize=1', token=token), 200, 'First lookup page')
second = expect(request('/catalog/products/search?page=2&pageSize=1', token=token), 200, 'Next lookup page')
verify(len(first['items']) == 1 and first['items'][0]['id'] != second['items'][0]['id'], 'Pagination loads distinct blocks')
expect(request('/catalog/products/search?page=0', token=token), 400, 'Invalid page rejected')
order = {'customerId': 'ALFKI', 'shipAddress': 'Main Street 123', 'shipCity': 'Quito', 'shipCountry': 'Ecuador',
         'shipPostalCode': '170101', 'orderDetails': [{'productId': product['id'], 'unitPrice': product['unitPrice'], 'quantity': 1}]}
expect(request('/CreateOrder', order), 401, 'Anonymous sale rejected')
duplicate = {**order, 'orderDetails': order['orderDetails'] * 2}
expect(request('/CreateOrder', duplicate, token), 400, 'Duplicate product rejected')
expect(request('/CreateOrder', {**order, 'customerId': 'ANTON'}, token), 400, 'Debtor rejected')
expect(request('/CreateOrder', {**order, 'orderDetails': [{**order['orderDetails'][0], 'quantity': product['unitsInStock'] + 1}]}, token), 400, 'Overselling rejected')
expect(request('/CreateOrder', {**order, 'orderDetails': [{**order['orderDetails'][0], 'unitPrice': product['unitPrice'] + 1}]}, token), 409, 'Tampered price rejected')
order_id = expect(request('/CreateOrder', order, token), 200, 'Sales transaction committed')
current = expect(request('/catalog/products', token=token), 200, 'Updated inventory')
remaining = next((p['unitsInStock'] for p in current if p['id'] == product['id']), 0)
verify(remaining == product['unitsInStock'] - 1, 'Inventory decreased automatically')
invoice = expect(request(f'/orders/{order_id}', token=token), 200, 'Saved invoice header and details')
money = lambda n: Decimal(str(n))
round_money = lambda n: n.quantize(Decimal('.01'), rounding=ROUND_HALF_UP)
subtotal = sum(money(d['unitPrice']) * d['quantity'] for d in invoice['details'])
discount = round_money(subtotal * money(invoice['discountPercent']) / 100)
vat = round_money((subtotal - discount) * money(invoice['vatRate']))
verify(money(invoice['total']) == subtotal - discount + vat, 'Server subtotal, discount, VAT and total')
other_email = 'other-' + uuid.uuid4().hex[:10] + '@example.test'
expect(request('/user/Register', {**registration, 'email': other_email}), 200, 'Second account')
other_token = expect(request('/user/Login', {'email': other_email, 'password': registration['password']}), 200, 'Second login')['accessToken']
expect(request(f'/orders/{order_id}', token=other_token), 404, 'Another user cannot read the invoice')
expect(request(f'/orders/{order_id}/invoice.pdf', token=other_token), 404, 'Another user cannot download the PDF')
available = expect(request('/catalog/products', token=token), 200, 'Special-order catalog')
verify(len(available) >= 4, 'At least four test products available')
special = {**order, 'orderDetails': [{'productId': p['id'], 'unitPrice': p['unitPrice'], 'quantity': 1} for p in available[:4]]}
special_id = expect(request('/CreateOrder', special, token), 200, 'Special order and configured notification')
pdf = expect(request(f'/orders/{special_id}/invoice.pdf', token=token), 200, 'Download invoice PDF')
verify(isinstance(pdf, bytes) and pdf.startswith(b'%PDF'), 'Actual PDF document returned')
output = Path(args.output)
output.mkdir(parents=True, exist_ok=True)
(output / 'invoice.pdf').write_bytes(pdf)
summary = {'checks': checks, 'health': health, 'orderId': order_id, 'specialOrderId': special_id, 'user': email}
(output / 'http-results.json').write_text(json.dumps(summary, indent=2), encoding='utf-8')
print(json.dumps(summary))
