using PacketTester;

namespace Program
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("1 = TCP Server\n2 = TCP Client\n3 = UDP Server\n4 = UDP Client");
            int l = int.Parse(Console.ReadLine());
            if (l == 1)
            {
                Console.Title = "Example TCP Server";
                string addr = Console.ReadLine();
                int port = int.Parse(Console.ReadLine());
                TcpServerClass.ClientObject.ServerInstance(addr, port);
            }
            else if (l == 2)
            {
                Console.Title = "Example TCP Client";
                string addr = Console.ReadLine();
                int port = int.Parse(Console.ReadLine());
                TcpClientClass.ClientInstance(addr, port);
            }
        }
    }
}