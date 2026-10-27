# Límites y caracteres de los formularios

La interfaz bloquea las entradas no permitidas al escribir y pegar. El backend
valida de nuevo los datos enviados, incluidos los que llegan desde Swagger.
Los campos vacíos se pueden editar, pero los obligatorios se rechazan al guardar.

| Campo | Caracteres y límites |
|---|---|
| Nombres y apellidos | Letras, tildes, ñ y espacios; obligatorios; máximo 30 caracteres cada uno. |
| Correo electrónico | En login y registro: letras ASCII, números y `. _ - + @`; una sola arroba; sin espacios ni nombre de presentación; máximo 30 caracteres. Parte anterior a la arroba: sin puntos consecutivos ni en los extremos. Dominio con al menos dos segmentos de hasta 63 caracteres, solo letras, números y guiones interiores. |
| Contraseña de registro | Sin máximo de longitud impuesto por la aplicación; al menos 6 caracteres; mayúscula, minúscula, número y símbolo. |
| Contraseña de login | Obligatoria; sin máximo de longitud impuesto por la aplicación. |
| Confirmación | Sin máximo de longitud; debe coincidir exactamente con la contraseña. |
| Dirección | Obligatoria; de 1 a 60 caracteres; letras, números, espacios y los signos `# . , / -`. |
| Ciudad y país | Obligatorios; de 3 a 15 caracteres; letras, tildes, ñ y espacios. Los espacios de los extremos no sirven para completar el mínimo. |
| Código postal | Opcional; hasta 10 dígitos ASCII; se conservan los ceros iniciales. |
| Cantidad | Número entero de 1 hasta las existencias del producto; máximo de almacenamiento 32767; sin signos, letras ni decimales. |
| Número de orden para consultar factura | Entero positivo de 1 a 2147483647; hasta 10 dígitos. |
| Búsqueda por campos | Código de cliente: hasta 5 letras o números; código de producto: entero positivo de hasta 10 dígitos; nombre: hasta 100 caracteres. Saldo y precio: de 0 a 999999,99, con dos decimales; existencias: de 1 a 32767. Se pueden combinar campos y rangos, manteniendo la paginación. |
| Cliente, producto, precio y totales | Se seleccionan del catálogo o se calculan. El usuario no edita códigos, precios ni totales directamente. |

La contraseña permite caracteres mixtos: la restricción de solo letras no se
aplica a contraseñas, correos, direcciones ni búsquedas.

Una entrada inválida se rechaza completa: pegar `Muñoz123` no se convierte
silenciosamente en otro apellido. La pantalla conserva el último valor admitido
y muestra la regla en español. Las cantidades vacías o inválidas no se guardan
utilizando silenciosamente la cantidad anterior.

Los límites de texto no superan la capacidad de las columnas existentes; esta actualización
no requiere migrar tablas ni modifica las facturas que ya están guardadas.

Al pulsar Quitar se muestra el producto en una confirmación. Cancelar o Escape conserva la línea y su cantidad; Sí, quitar elimina solo esa línea del borrador.

Los filtros consultan automáticamente después de una pausa de 400 ms. Las respuestas antiguas se descartan. Los avisos de validación y caracteres se agrupan sin duplicados en una única ventana emergente al frente con Entendido. No se muestran resúmenes de error arriba del formulario ni globos del navegador. Las cantidades conservan su validación y comunican el producto y el rango permitido en esa misma ventana.

El selector «Filtrar por» muestra solo el campo elegido. Los demás filtros se conservan
como etiquetas visibles y se pueden quitar de uno en uno. Limpiar los restablece todos.

El historial de facturas consulta las ventas persistidas del usuario autenticado, con
paginación de 5, 10 o 20 facturas. El backend limita las consultas a su identidad del token
y calcula los totales usando los precios, descuento e IVA guardados en la venta.
