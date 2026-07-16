namespace Nexas.Application.Common.Models.Bunny
{
    public class VideoUploadInformationDto
    {
        public string BunnyVideoId { get; set; } = string.Empty;
        
        /// <summary>
        /// A URL/Mecanismo oficial suportado pela Bunny Stream para realizar o upload direto (ex: TUS endpoint)
        /// </summary>
        public string UploadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Token ou headers necessários para autenticação temporária do upload (se fornecido oficialmente)
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
