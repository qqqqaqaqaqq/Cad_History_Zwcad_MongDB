using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.Commands
{
    public class NewWindowHost
    {
        public RelayCommand NewWindowHostCommand { get; }
        public RelayCommand NewCompareWindowHostCommand { get; }
        private readonly IWindowsService _windowsService;
        private readonly Action<System.Windows.Window>? OnWindowShow;
        private readonly IPdfService _pdfService;
        private byte[]? pdfbyte;
        private byte[]? comparepdfbyte;

        public NewWindowHost(
            IWindowsService windowsService,
            IPdfService pdfService,
            Action<System.Windows.Window> onHostChanged
            )
        {
            _windowsService = windowsService;
            _pdfService = pdfService;
            NewWindowHostCommand = new RelayCommand(NewWindowHostOpen);
            NewCompareWindowHostCommand = new RelayCommand(NewCompareWindowHostOpen);
            WeakReferenceMessenger.Default.Register<SendByteMessage>(this, async (r, m) =>
            {
                await SetPdf(m.Value);
            });
            WeakReferenceMessenger.Default.Register<SendCompareByteMessage>(this, async (r, m) =>
            {
                await SetComparePdf(m.Value);
            });
            OnWindowShow = onHostChanged;
        }
        public Task SetPdf(byte[] pdf)
        {
            pdfbyte = pdf;
            return Task.CompletedTask;
        }

        public Task SetComparePdf(byte[] pdf)
        {
            comparepdfbyte = pdf;
            return Task.CompletedTask;
        }


        public void NewCompareWindowHostOpen()
        {
            if (pdfbyte == null || comparepdfbyte == null) {
                WeakReferenceMessenger.Default.Send(new SendStatusMessage("Compare Image not Exist"));
                return; }

            List<Point> point = _pdfService.GetDifferences(pdfbyte, comparepdfbyte, 2000);

            // 이미지 => byte 변경
            byte[] result = _pdfService.AnnotatePdf(comparepdfbyte, point);

            WindowsFormsHost host = _pdfService.Host_Created(result);

            System.Windows.Window window = _windowsService.Form_View(host);

            WeakReferenceMessenger.Default.Send(new SendStatusMessage("Compare Image Succed"));
            OnWindowShow?.Invoke(window);
        }

        public void NewWindowHostOpen()
        {
            if (pdfbyte == null) {
                WeakReferenceMessenger.Default.Send(new SendStatusMessage("Image not Exist"));
                return; }

            WindowsFormsHost host = _pdfService.Host_Created(pdfbyte);

            // WindowFormsHost 는 깊은 복사 제공 안함.

            System.Windows.Window window = _windowsService.Form_View(host);

            WeakReferenceMessenger.Default.Send(new SendStatusMessage("WindowForm Show Succed"));
            OnWindowShow?.Invoke(window);
        }
    }
}
