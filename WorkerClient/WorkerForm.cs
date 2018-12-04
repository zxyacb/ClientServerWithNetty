using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Codecs.Protobuf;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using MessageProto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WorkerClient
{
    public partial class WorkerForm : Form
    {
        Bootstrap bootstrap;
        public WorkerForm()
        {
            InitializeComponent();
            var group = new MultithreadEventLoopGroup();
            bootstrap = new Bootstrap();

            // ssl证书设置
            X509Certificate2 cert = null;
            string targetHost = null;
            if (false)
            {
                cert = new X509Certificate2("server.pfx", "password");
                targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            }

            bootstrap.Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (cert != null)
                    {
                        pipeline.AddLast("tls", new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                    }
                    pipeline.AddLast(new IdleStateHandler(5,0,0),
                        new ProtobufVarint32FrameDecoder(),
                        new VarProtobufDecoder(),
                        new ProtobufVarint32LengthFieldPrepender(),
                        new VarProtobufEncoder());
                    pipeline.AddLast(new WorkerHandler());
                }));

        }
        IChannel clientChannel;
        private void button1_Click(object sender, EventArgs e)
        {
            clientChannel.WriteAndFlushAsync(new BasicMessage() { Message = "Hello" });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var task1 = bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8007)).ContinueWith((task) =>
            {
                clientChannel = task.Result;
            });
        }
    }
}
