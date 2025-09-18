using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MongoDB.Bson;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Commands
{
    public class CellEditEnding
    {
        public AsyncRelayCommand<object?> CellEditEndingCommand { get; }
        private readonly IEventEntryService _eventEntryService;

        public CellEditEnding(
            IEventEntryService eventEntryService)
        {
            _eventEntryService = eventEntryService;
            CellEditEndingCommand = new AsyncRelayCommand<object?>(OnEditing);
            WeakReferenceMessenger.Default.Register<SendObjectId>(this, async (r, m) =>
            {
                await Task.Run(() =>
                {
                    id = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendTime>(this, async (r, m) =>
            {
                await Task.Run(() =>
                {
                    time = m.Value;
                });
            });
        }

        private ObjectId id = new ObjectId();
        private DateTime time = DateTime.MinValue;

        public async Task OnEditing(object? parameter)
        {
            if (parameter == null)
            {
                WeakReferenceMessenger.Default.Send(new SendStatusMessage("Event Parameter Update Failure"));
                return;
            }
            EventList? selected = parameter as EventList;
            if (selected == null) { return; }
            if (selected.EventDescription == null) return;

            var evt = await _eventEntryService.FindAsync(id);
            var description = evt.EventCollection.Find(x => (x.Time - time).TotalMilliseconds < 100);
            if (description == null)
            {
                WeakReferenceMessenger.Default.Send(new SendStatusMessage("Event Description Update Failure"));
                return;
            }
            description.EventDescription = selected.EventDescription;

   
            await _eventEntryService.AddOrUpdateAsync(evt);
            WeakReferenceMessenger.Default.Send(new SendStatusMessage("Event Description Update Succed"));
        }
    }
}
