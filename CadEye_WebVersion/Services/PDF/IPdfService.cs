using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.Integration;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.PDF
{
    public interface IPdfService
    {
        // PDF 읽고 렌더링 하기
        Task<byte[]> RenderPdfPage(string path);

        // PDF 비교 후 포인트 반환
        List<Point> GetDifferences(byte[] targetBytes, byte[] sourceBytes, int RenderPixcel);

        // 이미지 => byte 변경
        byte[] AnnotatePdf(byte[] originalPdfBytes, List<Point> differences);

        // Host 생성
        WindowsFormsHost Host_Created(byte[] pdfBytes);
    }
}
