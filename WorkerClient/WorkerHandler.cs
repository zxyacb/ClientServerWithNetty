using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using MessageProto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerClient
{
    class WorkerHandler : ChannelHandlerAdapter
    {
        readonly IByteBuffer initialMessage;

        public WorkerHandler()
        {
            this.initialMessage = Unpooled.Buffer(1024);
            byte[] messageBytes = Encoding.ASCII.GetBytes("Hello world");
            this.initialMessage.WriteBytes(messageBytes);
            
        }

        public override void ChannelActive(IChannelHandlerContext context) {
            return;
            context.WriteAndFlushAsync(this.initialMessage);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message;
        }

        //public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        //{
        //    if(evt is IdleStateEvent)
        //    {
        //        IdleState state = ((IdleStateEvent)evt).State;
        //        if (state == IdleState.ReaderIdle)
        //        {
        //            context.WriteAndFlushAsync(new HeartBeatMessage());
        //            //throw new Exception("idle exception");
        //        }
        //    }
        //    else
        //    {
        //        base.UserEventTriggered(context, evt);
        //    }
        //}

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }

    public class SecureChatClientHandler : SimpleChannelInboundHandler<string>
    {
        protected override void ChannelRead0(IChannelHandlerContext contex, string msg) => Console.WriteLine(msg);

        public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
        {
            Console.WriteLine(DateTime.Now.Millisecond);
            Console.WriteLine(e.StackTrace);
            contex.CloseAsync();
        }
    }
}
