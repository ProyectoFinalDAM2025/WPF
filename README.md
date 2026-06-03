# Officium WPF

Aplicacion de escritorio desarrollada con WPF para la administracion de Officium. El proyecto funciona como panel interno para usuarios administradores y se comunica con la API REST local del backend.

Actualmente la app cubre dos areas principales:

- Gestion de administradores.
- Revision y moderacion de reportes sobre publicaciones, ofertas y perfiles.

## Estado actual

### Gestion de administradores

El modulo de usuarios incluye:

- Inicio de sesion contra la API.
- Acceso restringido a usuarios con rol `Administrador`.
- Persistencia local de sesion mediante `SettingsData`.
- Validacion de token al abrir la aplicacion y al navegar a pantallas internas.
- Cierre de sesion con limpieza de token y datos locales.
- Listado de administradores desde el backend.
- Vista en formato `DataGrid` y `ListView`.
- Busqueda/filtrado por rol, nombre, apellido, email y situacion.
- Prerregistro de administradores.
- Registro y verificacion por codigo.
- Creacion de perfil de administrador con datos personales y foto.
- Edicion de perfil de administrador.
- Consulta de informacion del perfil.
- Eliminacion de administradores, evitando eliminar el administrador de la sesion activa.
- Recuperacion y cambio de contrasena.
- Ventanas de notificacion para errores y respuestas del servidor.

### Gestion de reportes

El modulo de reportes permite al administrador revisar contenido denunciado desde la plataforma:

- Listado de reportes de publicaciones.
- Listado de reportes de ofertas de empleo.
- Listado de reportes de perfiles de usuario.
- Cambio entre vista `DataGrid` y `ListView`.
- Pantallas de detalle por tipo de reporte.
- Eliminacion de reportes sin afectar la entidad reportada.
- Moderacion de publicaciones reportadas.
- Moderacion de ofertas reportadas.
- Moderacion de perfiles reportados.
- Eliminacion administrativa de perfiles reportados.
- Recarga automatica del listado despues de moderar o eliminar.
- Manejo de sesion caducada al cargar o ejecutar acciones sobre reportes.

En reportes de publicaciones, la ventana de detalle tambien puede previsualizar archivos asociados:

- Imagenes.
- Videos, con controles de reproducir y pausar.
- PDF mediante navegador embebido.
- Apertura del documento en el visor externo del sistema.

## Tecnologias

- WPF
- C#
- .NET Framework 4.7.2
- MVVM parcial con `ViewModelBase`, comandos y view models por modulo
- `HttpClient` para comunicacion con API REST
- Newtonsoft.Json para serializacion y lectura flexible de respuestas JSON
- NuGet con `packages.config`

## Estructura principal

```text
WPF/
|-- app.sln
|-- app/
|   |-- App.xaml
|   |-- App.config
|   |-- Models/
|   |   |-- ApiRouteUsuario/
|   |   |-- IUsuariosRepository/
|   |   |-- Reportes/
|   |   `-- Usuarios/
|   |-- View/
|   |   |-- Home/
|   |   |-- Reportes/
|   |   |   |-- ReportesOfertas/
|   |   |   |-- ReportesPerfiles/
|   |   |   `-- ReportesPublicaciones/
|   |   `-- Usuarios/
|   |       |-- CambiarContrasena/
|   |       |-- EditarUsuarios/
|   |       |-- InformacionUsuarios/
|   |       |-- InicioDeSesion/
|   |       |-- MainUsuarios/
|   |       |-- Notificaciones/
|   |       |-- Pre_Registros/
|   |       |-- RecordarContrasenas/
|   |       `-- RegistroUsuarios/
|   |-- ViewModel/
|   |   |-- Reportes/
|   |   |-- Repositories/
|   |   `-- Usuarios/
|   `-- packages.config
`-- packages/
```

## Arquitectura

La aplicacion mantiene una organizacion cercana a MVVM:

- `View`: contiene las ventanas XAML y la interaccion de UI.
- `ViewModel`: concentra estado, comandos y llamadas principales a la API.
- `Models`: define entidades de usuarios, reportes, rutas de API e interfaces.
- `Repositories`: encapsula parte del acceso HTTP, especialmente en usuarios.
- `SettingsData`: guarda datos locales de sesion, como token, rol, id de perfil y nombre mostrado.

View models principales:

- `UsuarioViewModel`: autenticacion, sesion, administradores, registro, cambio de contrasena y operaciones de usuario.
- `ReportesViewModel`: carga reportes, normaliza respuestas JSON, abre detalles, elimina reportes, modera entidades y elimina perfiles reportados.

La clase `UsuarioViewModel` se usa como singleton mediante `UsuarioViewModel.Instance`. `ReportesViewModel` se instancia por pantalla de reportes.

## Flujo de navegacion

La aplicacion arranca en:

```text
View/Usuarios/InicioDeSesion/LogIn.xaml
```

Flujo principal:

1. `LogIn` valida si existe un token guardado.
2. Si hay token, se comprueba contra la API mediante `AccessToken`.
3. Si la sesion es valida y el rol es `Administrador`, se abre `Inicio`.
4. Desde `Inicio` se puede acceder a:
   - `MainUsuario`
   - `ReportesOfertas`
   - `ReportesPublicaciones`
   - `ReportesPerfiles`
5. Cada pantalla interna vuelve a validar la sesion al cargar datos protegidos.
6. Si el backend devuelve `Unauthorized` o `Forbidden`, la app limpia la sesion y vuelve a login.

## API

Las rutas estan centralizadas en:

```text
app/Models/ApiRouteUsuario/ApiRouteUsuarios.cs
```

URL base actual:

```text
https://api.officium.es/api
```

URL publica para archivos:

```text
https://api.officium.es
```

### Endpoints de sesion y usuarios

- `POST /login`
- `POST /logout`
- `GET /rolUsuario`
- `GET /user`
- `POST /register`
- `POST /pre-register`
- `POST /verifyCode`
- `POST /recover`
- `POST /change-password`
- `POST /Usuario/emailDisponible`
- `GET /administrador`
- `POST /administrador`
- `POST /administrador/{id}` con `_method=PUT`
- `DELETE /administrador/{id}`

### Endpoints de reportes

- `GET /reportes`
- `DELETE /reportes/{id}`
- `POST /reportes/{id}/moderar`
- `DELETE /reportes/{id}/entidad`

La moderacion se ejecuta en el backend. Al moderar:

- Publicaciones: se oculta el contenido, se eliminan archivos asociados y se marca el reporte como revisado.
- Ofertas: se sustituye el titulo/descripcion por texto de moderacion y la oferta queda cerrada.
- Perfiles: se sustituyen datos publicos del perfil por texto de moderacion.
- Perfiles eliminados: se borra el perfil reportado y sus reportes asociados.

El backend tambien puede generar notificaciones relacionadas con reportes moderados para el usuario que reporto y para el usuario reportado.

## Sesion y permisos

La app solo permite acceso a administradores. Durante el login se revisa el rol devuelto por la API. Si el usuario no es administrador, se muestra una notificacion de acceso denegado.

Los datos de sesion se guardan en:

- `token`
- `appToken`
- `idPerfil`
- `rol`
- `nombre`

Cuando el token caduca o la API devuelve un error de autorizacion, se limpian los datos locales y se redirige al inicio de sesion.

## Gestion de administradores

La pantalla `MainUsuario` permite:

- Cargar administradores activos/inactivos.
- Alternar entre tabla y lista.
- Filtrar por campos basicos.
- Abrir el formulario de prerregistro.
- Registrar usuarios.
- Editar un perfil existente.
- Consultar informacion del perfil.
- Eliminar un administrador seleccionado.
- Cambiar contrasena desde el menu de usuario.
- Cerrar sesion.

Actualmente las operaciones de edicion y borrado desde WPF estan enfocadas en administradores.

## Gestion de reportes

Las pantallas de reportes comparten `ReportesViewModel` y cargan todos los reportes desde la API, filtrando localmente por tipo:

- `ReportesOfertas`: filtra reportes de tipo `Oferta`.
- `ReportesPublicaciones`: filtra reportes de tipo `Publicacion`.
- `ReportesPerfiles`: filtra reportes de tipo `Usuario`.

Cada reporte puede abrir una ventana de detalle:

- `ReporteOfertaDetalle`
- `ReportePublicacionDetalle`
- `ReportePerfilDetalle`

Acciones disponibles:

- `Ver`: abre el detalle del reporte.
- `Eliminar reporte`: elimina solo el reporte.
- `Moderar`: oculta/modera la entidad reportada.
- `Eliminar perfil`: disponible en reportes de perfiles.

`ReportesViewModel` normaliza distintos nombres de campos del backend para tolerar respuestas con mayusculas, minusculas o nombres alternativos.

## Compilacion

Requisitos:

- Windows
- Visual Studio 2022 o compatible
- .NET Framework 4.7.2 Developer Pack
- Paquetes NuGet restaurados
- Backend disponible en `https://api.officium.es/api`

Compilar desde Visual Studio:

1. Abrir `WPF/app.sln`.
2. Restaurar paquetes NuGet si Visual Studio lo solicita.
3. Seleccionar configuracion `Debug` o `Release`.
4. Ejecutar el proyecto `app`.

Compilar con MSBuild:

```powershell
MSBuild.exe app.sln /p:Configuration=Debug /p:Platform="Any CPU" /m
```

## Configuracion local

La configuracion de mensajes y sesion esta definida en:

```text
app/App.config
app/SettingsData.settings
```

Si cambia la URL del backend, actualizar `BaseUrl` en:

```text
app/Models/ApiRouteUsuario/ApiRouteUsuarios.cs
```

Si cambia el dominio publico de archivos, actualizar `PublicBaseUrl` en el mismo archivo.

## Notas de desarrollo

- Las respuestas JSON y HTML de error se normalizan para mostrarse en la ventana `Notificacion`.
- La edicion de administrador usa `MultipartFormDataContent` para permitir envio de imagen.
- Los listados de administradores transforman la respuesta del backend a objetos `Administrador`, derivados de `UsuarioBase`.
- El modulo de reportes usa `JObject`/`JArray` para mapear respuestas de distintas entidades en un unico modelo `Reporte`.
- Las rutas de archivos se normalizan con `ApiRouteUsuarios.ResolvePublicFileUrl`, resolviendo rutas relativas de `storage` o `assets` contra `https://api.officium.es`.
- La vista de detalle de publicaciones detiene el reproductor de video al cerrar la ventana.
- El proyecto conserva codigo comentado y mensajes temporales de depuracion que pueden limpiarse en fases futuras.

## Proximos pasos sugeridos

- Extraer todas las llamadas HTTP a repositorios para dejar los view models mas ligeros.
- Mover la URL base de la API a configuracion externa.
- Unificar nombres de propiedades entre frontend y backend.
- Sustituir mensajes temporales de depuracion por notificaciones finales de UI.
- Anadir pruebas unitarias para sesion, transformacion de respuestas y acciones de reportes.
- Revisar encoding/nombres de carpetas con caracteres especiales para evitar problemas en herramientas de consola.
