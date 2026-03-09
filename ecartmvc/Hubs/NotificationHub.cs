using Microsoft.AspNetCore.SignalR;

namespace ecartmvc.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task RefreshProducts()
        {
            await Clients.All.SendAsync("RefreshProducts");
        }

        public async Task RefreshCategories()
        {
            await Clients.All.SendAsync("RefreshCategories");
        }

        public async Task ReceiveOrderUpdate(string orderId, string status)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", orderId, status);
        }
    }
}
