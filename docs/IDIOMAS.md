# Código en inglés e interfaz en español

Actualizado el 21 de octubre de 2026 según la aclaración del usuario.

- **Código en inglés:** proyectos, clases, métodos, variables, propiedades, rutas HTTP y nombres de campos JSON.
- **Interacción en español:** navegación, botones, formularios, validaciones, errores, confirmaciones, accesibilidad, correos de orden especial y facturas PDF.
- Los nombres originales de clientes y productos son datos del catálogo. NorthWind conserva su nombre propio. Los registros técnicos de desarrollo conservan el inglés.
- El documento HTML declara `lang="es"` y Blazor usa el formato `es-EC`. No necesitas activar la traducción automática de Chrome.

## Actualización aplicada en Debian

El 21 de octubre se actualizaron y compilaron las dos copias existentes. La API y Blazor están ejecutándose en segundo plano. Se conservaron la base de datos, el inventario, las credenciales y los archivos de configuración.

Abre http://localhost:5200 y recarga con Ctrl+F5. Swagger sigue en http://localhost:5100/swagger.

Para consultar estas ejecuciones desde Debian:

```bash
python3 /home/alex/NorthWind/tools/manage-debian-demo.py
python3 /home/alex/NorthWind-Full/NorthWind/tools/manage-debian-demo.py
```

Para detenerlas y después usar los comandos de inicio en tus terminales:

```bash
python3 /home/alex/NorthWind/tools/manage-debian-demo.py --stop
python3 /home/alex/NorthWind-Full/NorthWind/tools/manage-debian-demo.py --stop
```

Los comandos solo actúan sobre los procesos de prueba iniciados durante esta actualización. La base de datos permanece en ejecución. Los registros y archivos PID están en `artifacts/SpanishUpdate-20261021-122603` dentro de cada proyecto.

## Actualizar manualmente en el futuro


Los archivos de Windows ya están actualizados en el mismo proyecto. Para reflejarlos en tu Debian remoto, transfiere los ZIP actualizados. En PowerShell de Windows:

```powershell
scp "C:\Users\Usuario\Desktop\AplicacionesMoviles\Estudiantes\NorthWind-Backend-NET10-Debian.zip" "C:\Users\Usuario\Desktop\AplicacionesMoviles\Estudiantes\NorthWind-NET10.zip" alex@192.168.220.147:/home/alex/
```

Ingresa la contraseña SSH en tu terminal. Luego, en Debian, detén la API y Blazor con Ctrl+C en sus respectivas terminales y actualiza los archivos:

```bash
unzip -o /home/alex/NorthWind-Backend-NET10-Debian.zip -d /home/alex
unzip -o /home/alex/NorthWind-NET10.zip -d /home/alex/NorthWind-Full
```

Esto reemplaza los archivos de código existentes. Los ZIP excluyen la base de datos, credenciales locales y respaldos. El backend conserva sus 23 proyectos; la copia completa conserva los 31.

Reinicia cada aplicación desde su carpeta:

```bash
# Terminal del backend
cd /home/alex/NorthWind
bash tools/start-backend.sh
```

```bash
# Terminal de Blazor
cd /home/alex/NorthWind-Full/NorthWind
bash tools/start-frontend.sh
```

Recarga el navegador con Ctrl+F5 y selecciona Mostrar original si Chrome había traducido la página. Se mantiene el acceso habitual: Swagger en http://localhost:5100/swagger y Blazor en http://localhost:5200.

Para contenedores, reconstruye la imagen después de actualizar el código; `--no-build` reutiliza la imagen anterior.
