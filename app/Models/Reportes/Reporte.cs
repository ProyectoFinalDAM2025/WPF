namespace app.Models.Reportes
{
    public class Reporte
    {
        public string IDReporte { get; set; }
        public string TipoEntidad { get; set; }
        public string IDEntidad { get; set; }
        public string TituloEntidad { get; set; }
        public string Motivo { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string UsuarioReporta { get; set; }
        public string FechaCreacion { get; set; }
        public string OfertaTitulo { get; set; }
        public string OfertaDescripcion { get; set; }
        public string OfertaUbicacion { get; set; }
        public string OfertaEstado { get; set; }
        public string OfertaFechaPublicacion { get; set; }
        public string OfertaCategoria { get; set; }
        public string EmpresaNombre { get; set; }
        public string EmpresaUbicacion { get; set; }
        public string EmpresaSitioWeb { get; set; }
        public string EmpresaSector { get; set; }
        public string EmpresaFoto { get; set; }
        public string PublicacionContenido { get; set; }
        public string PublicacionFecha { get; set; }
        public string PublicacionGrupo { get; set; }
        public string PublicacionTipoArchivo { get; set; }
        public string PublicacionArchivo { get; set; }
        public string PublicacionThumbnail { get; set; }
        public string PublicacionPreview { get; set; }
        public string PublicacionAutor { get; set; }
        public string PublicacionAutorEmail { get; set; }
        public string PublicacionAutorFoto { get; set; }
    }
}
