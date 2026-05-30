# Officium WPF

Aplicacion de escritorio desarrollada con WPF para la gestion administrativa del proyecto Officium. Esta primera fase esta centrada en la gestion de usuarios, especialmente perfiles de administrador, autenticacion, sesion y operaciones de mantenimiento sobre usuarios.

## Estado actual

La fase de gestion de usuarios incluye:

- Inicio de sesion contra la API.
- Acceso restringido a usuarios con rol `Administrador`.
- Persistencia local de datos de sesion mediante `SettingsData`.
- Validacion de token al abrir la aplicacion y al navegar a pantallas internas.
- Cierre de sesion con limpieza de token y datos locales.
- Listado de administradores desde el backend.
- Vista de usuarios en formato `DataGrid` y `ListView`.
- Busqueda/filtrado por rol, nombre, apellido, email y situacion.
- Prerregistro de administradores.
- Registro y verificacion por codigo.
- Creacion de perfil de administrador con datos personales y foto.
- Edicion de perfil.
- Consulta de informacion de perfil.
- Eliminacion de administradores, evitando borrar el administrador de la sesion activa.
- Recuperacion y cambio de contrasena.
- Ventanas de notificacion para errores y respuestas del servidor.

## Tecnologias

- WPF
- C#
- .NET Framework 4.7.2
- MVVM parcial con `ViewModelBase`, comandos y repositorios
- `HttpClient` para comunicacion con API REST
- Newtonsoft.Json para serializacion y lectura de respuestas JSON
- MongoDB Driver incluido como dependencia del proyecto
- NuGet `packages.config`

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
|   |   `-- Usuarios/
|   |-- View/
|   |   |-- Home/
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
|   |   |-- Repositories/
|   |   `-- Usuarios/
|   `-- packages.config
`-- packages/
```

## Arquitectura

La aplicacion sigue una organizacion cercana a MVVM:

- `View`: contiene las ventanas XAML y su logica de interaccion.
- `ViewModel`: concentra estado, comandos y llamadas principales a la API.
- `Models`: define entidades de usuario, perfiles, rutas de API e interfaces de repositorio.
- `Repository`: encapsula parte del acceso HTTP a la API.
- `SettingsData`: almacena datos locales de sesion, como token, rol, id de perfil y nombre mostrado.

La clase principal de usuarios es `UsuarioViewModel`, implementada como singleton mediante `UsuarioViewModel.Instance`. Desde ahi se gestionan la autenticacion, carga de administradores, busqueda, cambio de contrasena, prerregistro, verificacion y cierre de sesion.

## Flujo de navegacion

La aplicacion arranca en:

```text
View/Usuarios/InicioDeSesion/LogIn.xaml
```

Flujo principal:

1. `LogIn` valida si ya existe un token guardado.
2. Si hay token, se comprueba contra la API mediante `AccessToken`.
3. Si la sesion es valida y el rol es `Administrador`, se abre `Inicio`.
4. Desde `Inicio` se accede al modulo `MainUsuario`.
5. `MainUsuario` carga y administra los perfiles de administrador.
6. Al cerrar sesion se llama a la API y se limpian los datos locales.

## API

Las rutas estan centralizadas en:

```text
app/Models/ApiRouteUsuario/ApiRouteUsuarios.cs
```

URL base actual:

```text
http://127.0.0.1:8000/api
```

Endpoints usados por la app:

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

Para ejecutar la aplicacion correctamente, el backend debe estar levantado en la URL configurada y debe exponer las rutas anteriores.

## Sesion y permisos

La app solo permite el acceso a administradores. Durante el login se revisa el campo `rol` devuelto por la API. Si el usuario no es administrador, se muestra una notificacion de acceso denegado.

Los datos de sesion se guardan en:

- `token`
- `appToken`
- `idPerfil`
- `rol`
- `nombre`

Cuando el token caduca o la API devuelve un error de autorizacion, se limpian los datos locales y el usuario vuelve al inicio de sesion.

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

Actualmente el repositorio bloquea roles distintos a `Administrador` en operaciones como borrado y edicion, por lo que la app queda enfocada en la gestion de administradores.

## Compilacion

Requisitos:

- Windows
- Visual Studio 2022 o compatible
- .NET Framework 4.7.2 Developer Pack
- Paquetes NuGet restaurados
- Backend disponible en `http://127.0.0.1:8000/api`

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

## Notas de desarrollo

- La comunicacion HTTP se realiza principalmente desde `UsuarioViewModel` y `RepositoryUsuario`.
- Las respuestas JSON y HTML de error se normalizan para mostrarse en la ventana `Notificacion`.
- La edicion de administrador usa `MultipartFormDataContent` para permitir envio de imagen.
- El listado de administradores transforma la respuesta del backend a objetos `Administrador`, derivados de `UsuarioBase`.
- El proyecto conserva codigo comentado de iteraciones anteriores que puede limpiarse en fases futuras.

## Proximos pasos sugeridos

- Extraer todas las llamadas HTTP a repositorios para dejar el `ViewModel` mas ligero.
- Mover la URL base de la API a configuracion externa.
- Unificar nombres de propiedades entre frontend y backend.
- Anadir pruebas unitarias para validacion, sesion y transformacion de respuestas.
- Revisar mensajes temporales de depuracion en ventanas como `Inicio`.
- Ampliar la gestion a otros roles si el alcance del proyecto lo requiere.
