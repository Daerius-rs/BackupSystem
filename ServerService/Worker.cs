using Network;
using SharedData;

namespace ServerService
{
    public class Server : BackgroundService
    {
        private ServerConnectionContainer _server;

        public async Task Listen()
        {
            _server = ConnectionFactory.CreateServerConnectionContainer(1708, false);
            _server.AllowUDPConnections = false;
            _server.ConnectionEstablished += (conn, type) =>
            {
                conn.RegisterStaticPacketHandler<SharedClass>(HandlerCommand);
            };

            await _server.Start();
        }
        private static async void HandlerCommand(SharedClass packet, Connection connection)
        {
            var shared = new SharedClass();
            switch (packet.Command)
            {

                case "backup":
                    {
                        shared.Command = "result";
                        
                        await File.AppendAllTextAsync("log.txt","->Get files for backup\n");

                        var files = FilesInfo.FromBin(packet.Files);
                        foreach (var file in files.Data)
                            await File.WriteAllBytesAsync($@"BackupFiles\{file.NameFile}", file.Bin);

                        shared.Value = "OK";
                        break;
                    }
                default:
                    await File.AppendAllTextAsync("log.txt",$"-> Command not found! ({packet.Command})");
                    break;
            }

            connection.Send(new SharedResponse(shared, packet));
        }
        protected internal void Disconnect() => _server.Stop();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Listen();
                }
                catch (Exception ex)
                {
                    // ��������� ������ ������������ ����������� ���������� ������� ������
                }

                await Task.Delay(5000);
            }
        }
    }
}