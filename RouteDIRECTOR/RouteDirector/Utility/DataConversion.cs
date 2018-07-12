using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	class DataConversion
	{
		/// <summary>
		/// 结构体转byte数组
		/// </summary>
		/// <param name="structObj">要转换的结构体</param>
		/// <returns>转换后的byte数组</returns>
		//public static byte[] StructToBytes(object structObj)
		//{
		//	//得到结构体的大小
		//	int size = Marshal.SizeOf(structObj);
		//	//创建byte数组
		//	byte[] bytes = new byte[size];
		//	//分配结构体大小的内存空间
		//	IntPtr structPtr = Marshal.AllocHGlobal(size);
		//	//将结构体拷到分配好的内存空间
		//	Marshal.StructureToPtr(structObj, structPtr, false);
		//	//从内存空间拷到byte数组
		//	Marshal.Copy(structPtr, bytes, 0, size);
		//	//释放内存空间
		//	Marshal.FreeHGlobal(structPtr);
		//	//返回byte数组
		//	return bytes;
		//}

		/// <summary>
		/// byte数组转结构体
		/// </summary>
		/// <param name="bytes">byte数组</param>
		/// <param name="type">结构体类型</param>
		/// <returns>转换后的结构体</returns>
		//public static object BytesToStuct(byte[] bytes, Type type)
		//{
		//	//得到结构体的大小
		//	int size = Marshal.SizeOf(type);
		//	//byte数组长度必须大于结构体的大小
		//	if (bytes.Length < size)
		//	{
		//		//返回空
		//		return null;
		//	}
		//	//分配结构体大小的内存空间
		//	IntPtr structPtr = Marshal.AllocHGlobal(size);
		//	//将byte数组拷到分配好的内存空间
		//	Marshal.Copy(bytes, 0, structPtr, size);
		//	//将内存空间转换为目标结构体
		//	object obj = Marshal.PtrToStructure(structPtr, type);
		//	//释放内存空间
		//	Marshal.FreeHGlobal(structPtr);
		//	//返回结构体
		//	return obj;
		//}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换后的数值</returns>
		public static T ByteToNum<T>(byte[] src, int offset, bool srcIsLittleEndian)
		{
			T res = default(T);
			int len = 0;
			byte[] buf = null;

			Action getBuf = (() =>
			{
				buf = new byte[len];
				Array.Copy(src, offset, buf, 0, len);
				if (BitConverter.IsLittleEndian != srcIsLittleEndian)
					Array.Reverse(buf, 0, len);
			});


			if (res is char)
			{
				len = sizeof(char);
				getBuf();
				return (T)(object)BitConverter.ToChar(buf, 0);
			}

			if (res is Int16)
			{
				len = sizeof(Int16);
				getBuf();
				return (T)(object)BitConverter.ToInt16(buf, 0);
			}

			if (res is Int32)
			{
				len = sizeof(Int32);
				getBuf();
				return (T)(object)BitConverter.ToInt32(buf, 0);
			}

			if (res is Int64)
			{
				len = sizeof(Int64);
				getBuf();
				return (T)(object)BitConverter.ToInt64(buf, 0);
			}

			if (res is UInt16)
			{
				len = sizeof(UInt16);
				getBuf();
				return (T)(object)BitConverter.ToUInt16(buf, 0);
			}

			if (res is UInt32)
			{
				len = sizeof(UInt32);
				getBuf();
				return (T)(object)BitConverter.ToUInt32(buf, 0);
			}

			if (res is UInt64)
			{
				len = sizeof(UInt64);
				getBuf();
				return (T)(object)BitConverter.ToUInt64(buf, 0);
			}

			if (res is Single)
			{
				len = sizeof(Single);
				getBuf();
				return (T)(object)BitConverter.ToSingle(buf, 0);
			}

			if (res is Double)
			{
				len = sizeof(Double);
				getBuf();
				return (T)(object)BitConverter.ToDouble(buf, 0);
			}
			
			return (T)(object)0;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref char num, bool srcIsLittleEndian)
		{
			int len = sizeof(char);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToChar(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref Int16 num, bool srcIsLittleEndian)
		{
			int len = sizeof(Int16);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToInt16(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref Int32 num, bool srcIsLittleEndian)
		{
			int len = sizeof(Int32);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToInt32(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref Int64 num, bool srcIsLittleEndian)
		{
			int len = sizeof(Int64);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToInt64(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref UInt16 num, bool srcIsLittleEndian)
		{
			int len = sizeof(UInt16);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToUInt16(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref UInt32 num, bool srcIsLittleEndian)
		{
			int len = sizeof(UInt32);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToUInt32(buf, 0);
			return len;
		}

		/// <summary>
		/// byte数组转数值
		/// </summary>
		/// <param name="src">byte数组</param>
		/// <param name="offset">偏移量</param>
		///  <param name="num">数值引用</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>转换字节数</returns>
		public static int ByteToNum(byte[] src, int offset, ref UInt64 num, bool srcIsLittleEndian)
		{
			int len = sizeof(UInt64);
			byte[] buf = new byte[len];
			Array.Copy(src, offset, buf, 0, len);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf, 0, len);
			num = BitConverter.ToUInt64(buf, 0);
			return len;
		}
		
		/// <summary>
		/// 数值转byte数组
		/// </summary>
		/// <param name="num">数值</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>byte数组</returns>
		public static byte[] NumToByte(Int16 num, bool srcIsLittleEndian)
		{
			byte[] buf = BitConverter.GetBytes(num);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf);
			return buf;
		}

		/// <summary>
		/// 数值转byte数组
		/// </summary>
		/// <param name="num">数值</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>byte数组</returns>
		public static byte[] NumToByte(Int32 num, bool srcIsLittleEndian)
		{
			byte[] buf = BitConverter.GetBytes(num);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf);
			return buf;
		}

		/// <summary>
		/// 数值转byte数组并追加
		/// </summary>
		/// <param name="num">数值</param>
		/// <param name="pre">要追加的byte数组</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>追加后byte数组</returns>
		public static byte[] NumToByte(Int16 num, byte[] pre, bool srcIsLittleEndian)
		{
			byte[] buf = BitConverter.GetBytes(num);
			if (pre == null)
				return NumToByte(num, srcIsLittleEndian);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf);
			pre = pre.Concat(buf).ToArray();
			return pre;
		}

		/// <summary>
		/// 数值转byte数组并追加
		/// </summary>
		/// <param name="num">数值</param>
		/// <param name="pre">要追加的byte数组</param>
		/// <param name="srcIsLittleEndian">数组字节序是否为小端模式</param>
		/// <returns>追加后byte数组</returns>
		public static byte[] NumToByte(Int32 num, byte[] pre, bool srcIsLittleEndian)
		{
			byte[] buf = BitConverter.GetBytes(num);
			if (pre == null)
				return NumToByte(num, srcIsLittleEndian);
			if (BitConverter.IsLittleEndian != srcIsLittleEndian)
				Array.Reverse(buf);
			pre = pre.Concat(buf).ToArray();
			return pre;
		}
	}
}
