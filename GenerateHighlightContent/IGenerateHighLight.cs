using System.Drawing;

namespace GenerateHighlightContent
{
    public interface IGenerateHighLight
    {
        /// <summary> 產生HighLight Code </summary>
        /// <returns> 產出的檔案路徑 </returns>
        string GenerateHighLightCode(HighLightParameter parameter);
    }
}
