export interface Customer {
  id: string
  name: string
  currentBalance: number
}
export interface Product {
  id: number
  name: string
  unitPrice: number
  unitsInStock: number
}
export interface Page<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
export interface SalesSettings {
  discountPercent: number
  vatRate: number
}
export interface Shipping {
  shipAddress: string
  shipCity: string
  shipCountry: string
  shipPostalCode: string
}
export interface CartLine {
  id: string
  product: Product
  quantity: string
}
export interface Account {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
}
export interface Tokens {
  accessToken: string
  expiresAt: string
}
export interface Session extends Tokens {
  fullName: string
  email: string
}
export interface Issue {
  field: string
  label: string
  message: string
}
export interface Filters {
  code: string
  name: string
  minimum: string
  maximum: string
  minStock: string
  maxStock: string
}
export interface Amounts {
  subtotal: number
  discountAmount: number
  taxableAmount: number
  vatAmount: number
  total: number
}
export interface InvoiceSummary {
  id: number
  orderDate: string
  customerId: string
  customerName: string
  productsCount: number
  total: number
}
export interface InvoiceLine {
  productId: number
  productName: string
  unitPrice: number
  quantity: number
  amount: number
}
export interface Invoice extends Amounts {
  id: number
  orderDate: string
  customerId: string
  customerName: string
  shipAddress: string
  shipCity: string
  shipCountry: string
  shipPostalCode: string | null
  createdBy: string
  discountPercent: number
  vatRate: number
  details: InvoiceLine[]
}
export type InputRule = 'letters' | 'email' | 'address' | 'digits' | 'money' | 'text'
