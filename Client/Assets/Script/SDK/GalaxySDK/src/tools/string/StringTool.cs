//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.1
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Text;
namespace SharkSDKTool
{
	public class StringTool
	{
		public StringTool ()
		{
		}
		public static string UrlEncode(string str)
		{

			StringBuilder sb = new StringBuilder();
			byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
			for (int i = 0; i < byStr.Length; i++)
			{
				sb.Append(@"%" + Convert.ToString(byStr[i], 16));
			}
			
			return (sb.ToString());
		}
	}

}

