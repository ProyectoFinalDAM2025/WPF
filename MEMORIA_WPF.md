# Memoria WPF - Officium

Fecha de actualizacion: 2026-06-03

Esta memoria resume el estado actual del proyecto WPF para servir como soporte a una memoria general del proyecto.

## Ubicacion

```text
C:\Users\srleo\Documents\GitHub\ProyectoFinalDAM2025\WPF
```

Solucion:

```text
WPF/app.sln
```

Proyecto principal:

```text
WPF/app/app.csproj
```

## Proposito

La app WPF funciona como panel administrativo de Officium. Esta orientada a usuarios con rol administrador y se comunica con la API REST del backend.

Actualmente cubre:

- Gestion de administradores.
- Revision, moderacion y eliminacion de reportes sobre publicaciones, ofertas y perfiles.

## Stack

- WPF
- C#
- .NET Framework 4.7.2
- Newtonsoft.Json
- HttpClient
- NuGet con `packages.config`
- Estructura cercana a MVVM

## Configuracion de API

Archivo central:

```text
app/Models/ApiRouteUsuario/ApiRouteUsuarios.cs
```

Estado actual:

```text
BaseUrl = https://api.officium.es/api
PublicBaseUrl = https://api.officium.es
```

`ApiRouteUsuarios.ResolvePublicFileUrl` resuelve rutas de archivos:

- Si la ruta ya es `http` o `https`, la devuelve intacta.
- Si empieza por `storage/` o `assets/`, la concatena con `PublicBaseUrl`.
- Si es relativa simple, la resuelve como `PublicBaseUrl/storage/{ruta}`.

Este cambio sustituyo usos anteriores de `http://127.0.0.1:8000` en el flujo de reportes.

## Sesion

La sesion se guarda en `SettingsData`:

- `token`
- `appToken`
- `idPerfil`
- `rol`
- `nombre`

La app valida el token con:

```text
GET /rolUsuario
```

Si la API responde `Unauthorized` o `Forbidden`, se limpia la sesion y se vuelve a login.

## Navegacion principal

La app arranca en:

```text
app/View/Usuarios/InicioDeSesion/LogIn.xaml
```

Despues del login valido con rol `Administrador`, abre:

```text
app/View/Home/Inicio.xaml
```

Desde `Inicio` se navega a:

- `MainUsuario` para administradores.
- `ReportesOfertas`.
- `ReportesPublicaciones`.
- `ReportesPerfiles`.

Archivo de navegacion del home:

```text
app/View/Home/Inicio.xaml.cs
```

Nota: `Inicio.Window_Loaded` aun muestra `MessageBox` de depuracion al validar la sesion.

## Gestion de administradores

View model principal:

```text
app/ViewModel/Usuarios/UsuarioViewModel.cs
```

Repositorio parcial:

```text
app/ViewModel/Repositories/RepositoryUsuarios/RepositoryUsuario.cs
```

Pantalla principal:

```text
app/View/Usuarios/MainUsuarios/MainUsuario.xaml
```

Funciones cubiertas:

- Login restringido a administradores.
- Carga de administradores.
- Vista `DataGrid` y `ListView`.
- Busqueda por campos.
- Prerregistro.
- Registro/verificacion.
- Creacion de perfil administrador.
- Edicion.
- Consulta de informacion.
- Eliminacion.
- Cambio y recuperacion de contrasena.
- Logout.

Endpoints principales:

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

## Gestion de reportes

Modelo:

```text
app/Models/Reportes/Reporte.cs
```

View model:

```text
app/ViewModel/Reportes/ReportesViewModel.cs
```

Pantallas:

```text
app/View/Reportes/ReportesOfertas/
app/View/Reportes/ReportesPublicaciones/
app/View/Reportes/ReportesPerfiles/
```

El `ReportesViewModel`:

- Carga reportes desde `GET /reportes`.
- Filtra localmente por tipo (`Oferta`, `Publicacion`, `Usuario`).
- Normaliza nombres de campos alternativos en JSON.
- Mapea datos de entidad, usuario, empresa, categoria, sector, publicacion, archivos y perfil.
- Abre ventanas de detalle segun `TipoEntidad`.
- Ejecuta eliminar reporte, moderar reporte y eliminar perfil.
- Recarga el listado despues de cada accion exitosa.

Endpoints de reportes:

- `GET /reportes`
- `DELETE /reportes/{id}`
- `POST /reportes/{id}/moderar`
- `DELETE /reportes/{id}/entidad`

## Detalles de reportes

Ofertas:

```text
app/View/Reportes/ReportesOfertas/ReporteOfertaDetalle.xaml
```

Acciones:

- Moderar.
- Eliminar reporte.
- Cerrar.

Publicaciones:

```text
app/View/Reportes/ReportesPublicaciones/ReportePublicacionDetalle.xaml
```

Acciones:

- Moderar.
- Eliminar reporte.
- Cerrar.
- Abrir documento externo.
- Reproducir/pausar video.

Previsualizacion soportada:

- Foto.
- Video.
- PDF.

La ventana detiene el `MediaElement` al cerrar.

Perfiles:

```text
app/View/Reportes/ReportesPerfiles/ReportePerfilDetalle.xaml
```

Acciones:

- Moderar.
- Eliminar reporte.
- Eliminar perfil reportado.
- Cerrar.

## Relacion con backend

La moderacion real ocurre en la API. Desde WPF solo se dispara la accion.

Comportamiento esperado del backend al moderar:

- Publicacion: elimina/oculta archivos y sustituye el contenido por texto moderado.
- Oferta: sustituye titulo/descripcion y cierra la oferta.
- Perfil: sustituye datos publicos por texto moderado.
- Reporte: queda en estado `Revisado`.
- Notificaciones: la API crea notificaciones para el usuario que reporto y para el usuario reportado/dueno del contenido.

## Documentacion actual

El README actualizado esta en:

```text
WPF/README.md
```

Incluye:

- Estado del panel administrativo.
- Gestion de administradores.
- Gestion de reportes.
- Estructura.
- Arquitectura.
- Endpoints.
- Compilacion.
- Configuracion local.
- Notas y proximos pasos.

## Verificacion reciente

Compilacion usada:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" app.sln /p:Configuration=Debug /p:Platform="Any CPU" /p:OutDir="C:\Users\srleo\Documents\GitHub\ProyectoFinalDAM2025\WPF\tmp_build\" /m
```

Resultado anterior conocido:

```text
Compilacion correcta.
0 Advertencia(s)
0 Errores
```

## Puntos de cuidado

- Hay nombres de carpetas con caracteres especiales, como las carpetas reales de cambiar contrasena y recordar contrasena, que pueden verse mal en algunas terminales.
- Algunos textos muestran mojibake en codigo. El README y esta memoria se mantienen en ASCII para evitar problemas.
- `Inicio.Window_Loaded` todavia usa `MessageBox` de depuracion.
- El codigo conserva comentarios y bloques antiguos.
- `ReportesViewModel` mezcla mapeo JSON, HTTP y UI; podria extraerse a repositorio/servicio.
- `UsuarioViewModel` tambien concentra muchas responsabilidades.

## Pendientes sugeridos

- Limpiar mensajes temporales de depuracion en `Inicio`.
- Extraer servicios/repositories para reportes.
- Unificar nombres de propiedades con backend.
- Revisar encoding del codigo fuente y textos.
- Agregar pruebas para mapeo de reportes y acciones de moderacion.
- Evaluar mover `BaseUrl` y `PublicBaseUrl` a configuracion externa.
