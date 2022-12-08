﻿using System;
using System.ServiceProcess;
using Microsoft.Win32;
using Windows.Media.Devices;
class Program
{
	static void Main(string[] args)
	{
		var toggle = new ToggleAudioDisableAllEnhancements();
		toggle.Run();
	}
}
class ToggleAudioDisableAllEnhancements
{
	const string audioRender = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render";
	const string disableAllEnhancements = "{1da5d803-d492-4edd-8c23-e0c0ffee7f0e},5";
	public void Run()
	{
		var dev = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
		var reg = FindRegistry(dev);
		ToggleRegistry(reg);
		RestartService("Audiosrv");
		Console.ReadKey();
	}
	void ToggleRegistry(string inRegistry)
	{
		if (string.IsNullOrEmpty(inRegistry))
		{
			return;
		}
		var regName = $@"{audioRender}\{inRegistry}\FxProperties";
		Console.WriteLine($"Registry : {regName}");
		var reg = Registry.LocalMachine.OpenSubKey(regName, true);
		var val = (Int32)reg.GetValue(disableAllEnhancements) == 0 ? 1 : 0;
		reg.SetValue(disableAllEnhancements, val);
		if ((Int32)reg.GetValue(disableAllEnhancements) == 1)
		{
			Console.WriteLine("disableAllEnhancements is on");
		}
		else
		{
			Console.WriteLine("disableAllEnhancements is off");
		}
	}
	string FindRegistry(string inName)
	{
		var reg = Registry.LocalMachine.OpenSubKey(audioRender);
		var keys = reg.GetSubKeyNames();
		foreach (var key in keys)
		{
			var prop = Registry.LocalMachine.OpenSubKey($@"{audioRender}\{key}\Properties");
			var names = prop.GetValueNames();
			foreach (var name in names)
			{
				var val = prop.GetValue(name) as string;
				if (inName == val)
				{
					return key;
				}
			}
		}
		return null;
	}
	void RestartService(string inServiceName)
	{
		var services = ServiceController.GetServices();
		ServiceController audio = null;
		foreach (var service in services)
		{
			if (service.ServiceName == inServiceName)
			{
				audio = service;
				break;
			}
		}
		audio.Stop();
		audio.WaitForStatus(ServiceControllerStatus.Stopped);
		audio.Start();
		audio.WaitForStatus(ServiceControllerStatus.Running);
	}
}