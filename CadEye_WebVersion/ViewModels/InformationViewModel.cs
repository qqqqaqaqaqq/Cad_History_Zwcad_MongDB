using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using MongoDB.Bson;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.ViewModels
{
    public class InformationViewModel : INotifyPropertyChanged
    {
        #region Init Fields
        private ObservableCollection<EventList> _informationEvent = new ObservableCollection<EventList>();
        private ObservableCollection<string> _refs = new ObservableCollection<string>();
        public CellEditEnding CellEdit { get; }
        public EventList? _selectedEventEntry;
        private WindowsFormsHost? _host;
        private WindowsFormsHost? _historyhost;
        private IEventEntryService _eventEntryService;
        private IImageEntryService _imageEntryService;
        private IRefEntryService _refEntryService;
        private IPdfService _pdfService;
        private IWindowsService _windowsService;
        private byte[]? _pdfbyte;
        private byte[]? _comparebyte;
        private ObjectId _id;
        #endregion

        #region Properties 
        public ObjectId Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<EventList> InformationEvent
        {
            get => _informationEvent;
            set
            {
                _informationEvent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Ref
        {
            get => _refs;
            set
            {
                _refs = value;
                OnPropertyChanged();
            }
        }

        public EventList? SelectedEventEntry
        {
            get => _selectedEventEntry;
            set
            {
                _selectedEventEntry = value;
                OnPropertyChanged();

                if (SelectedEventEntry != null)
                {
                    EventHistoryView(SelectedEventEntry.Time);
                    WeakReferenceMessenger.Default.Send(new SendTime(SelectedEventEntry.Time));
                }
            }
        }

        public WindowsFormsHost? Host
        {
            get => _host;
            set
            {
                _host = value;
                OnPropertyChanged();
            }
        }

        public WindowsFormsHost? HistoryHost
        {
            get => _historyhost;
            set
            {
                _historyhost = value;
                OnPropertyChanged();
            }
        }

        public byte[]? PdfByte
        {
            get => _pdfbyte;
            set
            {
                _pdfbyte = value;
                OnPropertyChanged();
                if (_pdfbyte != null)
                {
                    WeakReferenceMessenger.Default.Send(new SendByteMessage(_pdfbyte));
                    WeakReferenceMessenger.Default.Send(new SendCompareByteMessage(null!));
                }
            }
        }

        public byte[]? ComparePdfByte
        {
            get => _comparebyte;
            set
            {
                _comparebyte = value;
                OnPropertyChanged();
                if (_comparebyte != null)
                    WeakReferenceMessenger.Default.Send(new SendCompareByteMessage(_comparebyte));
            }
        }
        #endregion
        
        public InformationViewModel(
            IEventEntryService eventEntryService,
            IImageEntryService imageEntryService,
            IPdfService pdfService,
            IWindowsService windowsService,
            IRefEntryService refEntryService)
        {
            #region Fields
            _eventEntryService = eventEntryService;
            _imageEntryService = imageEntryService;
            _windowsService = windowsService;
            _pdfService = pdfService;
            _refEntryService = refEntryService;
            CellEdit = new CellEditEnding(eventEntryService);
            WindoHostCommand = new NewWindowHost(_windowsService, pdfService, OpenWindowForm);
            #endregion

            #region Message
            WeakReferenceMessenger.Default.Register<SendObjectId>(this, async (r, m) =>
            {
                await SelectItemEvent(m.Value);
            });
            #endregion
        }

        private async void EventHistoryView(DateTime time)
        {
            // Refs 리스트 등록
            var matchingid_refs = await _refEntryService.FindAsync(Id);
            if (matchingid_refs == null) return;
            var collection_refs = matchingid_refs.Refs
                .Find(x => x.Time.HasValue && Math.Abs((x.Time.Value - time).TotalMilliseconds) < 100);
            if (collection_refs == null) { return; }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (collection_refs.Ref == null) return;

                Ref.Clear();
                foreach (var item in collection_refs.Ref)
                {
                    Ref.Add(item);
                }
            });

            // 이미지 리스트 등록
            var matchingid_image = await _imageEntryService.FindAsync(Id);
            if (matchingid_image == null) return;
            var collection_image = matchingid_image.Path
               .Find(x => x.Time.HasValue && Math.Abs((x.Time.Value - time).TotalMilliseconds) < 100);
            if (collection_image == null) { return; }
            if (collection_image.ImagePath == null) { return; }
            byte[] pdfbyte = await _pdfService.RenderPdfPage(collection_image.ImagePath);

            // PDF를 ICommand -> NewWWinodwHost에 전달
            ComparePdfByte = pdfbyte;

            WindowsFormsHost host = _pdfService.Host_Created(pdfbyte);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                HistoryHost = null;
                if (host != null)
                {
                    HistoryHost = host;
                }
            });

            WeakReferenceMessenger.Default.Send(new SendStatusMessage("History Information loaded successfully"));
        }


        private async void OpenWindowForm(System.Windows.Window window)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    window.Show();
                });
        }

        public async Task SelectItemEvent(ObjectId id)
        {
            if (id == ObjectId.Empty) return;

            Id = id;

            // 이벤트 리스트 등록
            var matchingid_event = await _eventEntryService.FindAsync(id);
            if (matchingid_event == null) return;
            var collection_event = matchingid_event.EventCollection.ToList();
            if (collection_event == null) { return; }

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                InformationEvent.Clear();
                foreach (var item in collection_event)
                {
                    InformationEvent.Add(item);
                }
            });

            // 이미지 리스트 등록
            var matchingid_image = await _imageEntryService.FindAsync(id);
            if (matchingid_image == null) return;
            var collection_image = matchingid_image.Path.OrderBy(p => p.Time).Last();
            if (collection_image == null) { return; }
            if (collection_image.ImagePath == null) { return; }
            byte[] pdfbyte = await _pdfService.RenderPdfPage(collection_image.ImagePath);

            // PDF를 ICommand -> NewWWinodwHost에 전달
            PdfByte = pdfbyte;

            WindowsFormsHost host = _pdfService.Host_Created(pdfbyte);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Host = null;
                HistoryHost = null;
                if (host != null)
                {
                    Host = host;
                }
            });

            WeakReferenceMessenger.Default.Send(new SendStatusMessage("File Information loaded successfully"));
        }

        public NewWindowHost WindoHostCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
