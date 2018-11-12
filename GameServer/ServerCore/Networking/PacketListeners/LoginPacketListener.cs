﻿using Common.Networking.Packets;
using CommonCode.EventBus;
using ServerCore.Networking;
using ServerCore.Networking.NetworkEvents;
using Storage.Login;

namespace ServerCore.PacketListeners
{
    public class LoginPacketListener : IEventListener
    {
        [EventMethod]
        public void OnLogin(LoginPacket packet)
        {
            var client = ServerTcpHandler.GetClient(packet.ClientId);
            try
            {
                // Check if already is online
                string userId = LoginDao.GetUserId(packet.Login);
                var player = Server.GetPlayer(userId);
                if (player != null)
                {
                    client.Send(new DialogPacket()
                    {
                        Title = "Error",
                        Message = "Account Already Online"
                    });
                    return;
                }

                var user = AccountService.Login(packet.Login, packet.Password);
                Log.Info("Sending Login Response");

                client.Send(new LoginResponsePacket()
                {
                    SessionId = user.SessionId,
                    UserId = user.UserId,
                    xLocation = user.X,
                    yLocation = user.Y
                });

                client.Authenticated = true;

                ServerEvents.Call(new PlayerLoggedInEvent()
                {
                    Player = user,
                    Client = client
                });
            }
            catch (AccountError e)
            {
                Log.Info("Sending Login Error");
                client.Send(new DialogPacket()
                {
                    Title = "Acount Error",
                    Message = e.ErrorMessage
                });

            }
        }
    }
}