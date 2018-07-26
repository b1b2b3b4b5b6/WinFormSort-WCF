using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using static RouteDirector.MessageBase;
using static RouteDirector.ReportInfo;
namespace RouteDirector
{
	public class RouteDirectControl
	{
		Thread receiveThread;
		TCPSocket tcpSocket;
		System.Timers.Timer recHeartTime;
		System.Timers.Timer sendHeartTime;

		private Queue recMsgQuene = new Queue();
		Semaphore recMsgCount = new Semaphore(0, 1000);

		static readonly object sendLock = new object();
		public static Int16 heartBeatTime = 5;
		static Int16 cycleNum = 0;
		static Int16 ack = 0;
		public bool online = false;


		public RouteDirectControl()
		{
			tcpSocket = new TCPSocket();
			receiveThread = new Thread(ReceiveHandle) { IsBackground = true };
			RecHeartTimerInit(heartBeatTime * 2 + 1);
			SendHeartTimerInit(heartBeatTime);
		}
		/// <summary>
		/// 建立一次连接
		/// </summary>
		/// <param name="ip">ip字符</param>
		/// <param name="port">port字符</param>
		/// <returns>连接状态</returns>
		private int EstablishConnection()
		{

			IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("172.16.18.171"), Convert.ToInt32("3000"));
			Log.log.Info("Try to establish connection");
			if (tcpSocket.ConnectServer(ipe) == 0)
			{
				RecHeartTimerStart();
				receiveThread.Start();
				SendStart();
				Thread.Sleep(100);
				if (online == false)
				{
					Log.log.Info("Connection fail");
					return -1;
				}
				SendHeartTimerStart();
				Log.log.Info("Establish connection success");
				return 0;
			}
			Log.log.Info("Unable Connect with device");
			return -1;
		}

		#region  心跳监视


		private void RecHeartTimerInit(int s)
		{
			Log.log.Debug("Receive heartbeat init");
			recHeartTime = new System.Timers.Timer();
			recHeartTime.Elapsed += RecHeartTimer_Elapsed;
			recHeartTime.Interval = s * 1000;
			recHeartTime.AutoReset = false;
			recHeartTime.Stop();
		}
		private void RecHeartTimerReset()
		{
			recHeartTime.Stop();
			recHeartTime.Start();
			Log.log.Debug("Receive heartbeat reset");
		}
		private void RecHeartTimerStop()
		{
			Log.log.Debug("Receive heartbeat stop");
			recHeartTime.Stop();
		}
		private void RecHeartTimerStart()
		{
			Log.log.Debug("Receive heartbeat start");
			recHeartTime.Start();
		}
		private void RecHeartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Log.log.Debug("Receive heartbeat break，break connection");
			ContinueConnection();
		}

		private void SendHeartTimerInit(int s)
		{
			Log.log.Debug("Send heartbeat init");
			sendHeartTime = new System.Timers.Timer();
			sendHeartTime.Elapsed += SendHeartTimer_Elapsed;
			sendHeartTime.Interval = s * 1000;
			sendHeartTime.AutoReset = true;
			sendHeartTime.Stop();
		}

		private void SendHeartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			HeartBeat heartBeat = new HeartBeat(heartBeatTime);
			Log.log.Debug("Send Heartbeat");
			SendMsg(heartBeat);
		}

		private void SendHeartTimerStop()
		{
			Log.log.Debug("Send heartbeat stop");
			sendHeartTime.Stop();
		}

		private void SendHeartTimerStart()
		{
			Log.log.Debug("Send heartbeat start");
			sendHeartTime.Start();
		}

		private void SendHeartTimerReset()
		{
			sendHeartTime.Stop();
			sendHeartTime.Start();
			Log.log.Debug("Send heartbeat reset");
		}

		#endregion

		/// <summary>
		/// 断开连接
		/// </summary>
		public void StopConnection()
		{
			online = false;
			ack = 0;
			RecHeartTimerStop();
			SendHeartTimerStop();
			receiveThread.Abort();
			//缺少对receive是否完成的判断
			receiveThread = new Thread(ReceiveHandle) { IsBackground = true };
			tcpSocket.DisconnectServer();
			Log.log.Info("StopConnection success");

		}

		/// <summary>
		/// 重复建立连接直到成功
		/// </summary>
		public void ContinueConnection()
		{
			int count = 0;
			while (true)
			{
				StopConnection();
				if (EstablishConnection() == 0)
				{
					Log.log.Info("Connection success");
					break;
				}
				else
				{
					Log.log.Info("Connection fail,try again");

					count++;
				}
				if (count >= 3)
				{
					ReportError(new ErrorInfo(ReportInfo.ErrorInfo.ErrorCode.ConnectionFalut,"",0,0));
				}
				Log.log.Info("Connectioning,wait 10s");
				Thread.Sleep(10000);
			}

		}

		private void Unexpect()
		{
			Log.log.Error("Unexpect msg");
			throw new Exception("unexpect msg");
		}

		private void ReceiveHandle()
		{
			while (true)
			{
				byte[] packetBuf;
				packetBuf = tcpSocket.ReceiveData();
				if (packetBuf == null)
				{
					break;
				}
				RecHeartTimerReset();
				PacketResolve(packetBuf);
			}
		}

		private void PacketResolve(byte[] packetBuf)
		{
			int start = 0;
			int end = 0;
			int len = packetBuf.Length;
			if (packetBuf[len - 1] != 0xff)
				throw new NotImplementedException();
			if (packetBuf[len - 2] != 0xff)
				throw new NotImplementedException();
			if (packetBuf[len - 3] != 0xff)
				throw new NotImplementedException();
			if (packetBuf[len - 4] != 0xff)
				throw new NotImplementedException();
			while (true)
			{
				if (packetBuf[end] == 0xff)
				{
					end++;
					if (packetBuf[end] == 0xff)
					{
						end++;
						if (packetBuf[end] == 0xff)
						{
							end++;
							if (packetBuf[end] == 0xff)
							{
								end++;
								byte[] qPacketBuf = new byte[end - start];
								Array.Copy(packetBuf, start, qPacketBuf, 0, end - start);
								start = end;

								Packet packet = new Packet(qPacketBuf);
								Log.log.Debug(packet.GetInfo(new StringBuilder("receive packet\r\n")));
								if (packet.cycleNum != 0)
									ack = packet.cycleNum;
								else
									online = true;
								foreach (MessageBase msg in packet.messageList)
								{
									if (msg.msgId == (Int16)MessageType.HeartBeat)
									{
										Log.log.Info("Connnection is OK");
										//SendMsg(new HeartBeat(heartBeatTime));
										//break;
									}


									if (msg.msgId == (Int16)MessageType.CommsErr)
									{
										Log.log.Error("Get CommsErr");
										ContinueConnection();
									}

									if (msg.msgId == (Int16)MessageType.NodeAva)
									{
										Unexpect();
									}

									if (msg.msgId == (Int16)MessageType.NoType)
									{
										Unexpect();
									}

									recMsgQuene.Enqueue(msg);
									recMsgCount.Release();

								}
								if (end == packetBuf.Length)
									break;
							}
							else
								end++;
						}
						else
							end++;
					}
					else
						end++;
				}
				else
					end++;
			}
		}

		private void SendStart()
		{
			Packet packet = new Packet();
			HeartBeat heartBeat = new HeartBeat(heartBeatTime);
			packet.AddMsg(heartBeat);
			packet.AddCycleNum(0, 0);
			tcpSocket.SendData(packet.GetBuf());
			Log.log.Debug("Send start");
			cycleNum = 1;
		}

		/// <summary>
		/// 等待接收一条消息
		/// </summary>
		public MessageBase WaitMsg()
		{
			recMsgCount.WaitOne();

			return (MessageBase)recMsgQuene.Dequeue();
		}

		/// <summary>
		/// 查询接收一条消息
		/// </summary>
		public MessageBase GetMsg()
		{
			bool res = recMsgCount.WaitOne(1);
			if (res == true)
				return (MessageBase)recMsgQuene.Dequeue();
			else
				return null;
		}

		/// <summary>
		/// 发送一个消息
		/// </summary>
		/// <param name="msg">消息对象</param>
		public void SendMsg(MessageBase msg)
		{
			lock (sendLock)
			{
				if (online == false)
				{
					Log.log.Debug("Connection break,can not send msg");
					return;
				}
				Packet packet = new Packet();
				packet.AddCycleNum(cycleNum, ack);
				packet.AddMsg(msg);
				if(tcpSocket.SendData(packet.GetBuf()) <= 0)
				{
					Log.log.Error("Send msg fail");
					return;
				}
				SendHeartTimerReset();
				Log.log.Debug(packet.GetInfo(new StringBuilder("send msg\r\n")));
				cycleNum++;
				if (cycleNum > 99)
					cycleNum = 1;
			}

		}


		/// <summary>
		/// 清空消息
		/// </summary>
		public void FlushMsg()
		{
			Thread.Sleep(2000);
			while (true)
			{
				MessageBase msg = new MessageBase();
				msg = GetMsg();
				if (msg == null)
					break;
				else
					Log.log.Debug("abandon last msg");
			}
		}
	}
}
