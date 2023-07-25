using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exiled.API.Features;
using MapEditorReborn.API.Extensions;
using UnityEngine;

namespace PlayhousePlugin.Components
{
	public class PlayhousePluginComponent : MonoBehaviour
	{
		public Player player { get; private set; }

		private string _hudTemplate =
			"<line-height=95%><voffset=8.5em><align=left><size=50%><alpha=#44>[STATS]<alpha=#ff></size></align>\n<align=right>[LIST]</align><align=center>[CENTER_UP][CENTER][CENTER_DOWN][BOTTOM]";

		private float _timer = 0f;
		private Tip _proTip;
		private int _timerCount = 0;
		private string _hudText = string.Empty;

		private string _hudCenterUpString = string.Empty;
		private float _hudCenterUpTime = -1f;
		private float _hudCenterUpTimer = 0f;

		private string _hudCenterString = string.Empty;
		private float _hudCenterTime = -1f;
		private float _hudCenterTimer = 0f;

		private string _hudCenterDownString = string.Empty;
		private float _hudCenterDownTime = -1f;
		private float _hudCenterDownTimer = 0f;

		public static List<PlayhousePluginComponent> Instances = new List<PlayhousePluginComponent>();

		//private string _hudBottomDownString = string.Empty;
		//private float _hudBottomDownTime = -1f;
		//private float _hudBottomDownTimer = 0f;

		private void Start()
		{
			player = Player.Get(gameObject);
			Instances.Add(this);
			//_hudTemplate = _hudTemplate.Replace("[VERSION]", $"Ver{}");
		}

		public void AddHudCenterUpText(string text, ulong timer)
		{
			_hudCenterUpString = text;
			_hudCenterUpTime = timer;
			_hudCenterUpTimer = 0f;
		}

		public void ClearHudCenterUpText()
		{
			_hudCenterTime = -1f;
		}

		public void AddHudCenterText(string text, ulong timer)
		{
			_hudCenterString = text;
			_hudCenterTime = timer;
			_hudCenterTimer = 0f;
		}

		public void ClearHudCenterText()
		{
			_hudCenterTime = -1f;
		}

		public void AddHudCenterDownText(string text, ulong timer)
		{
			_hudCenterDownString = text;
			_hudCenterDownTime = timer;
			_hudCenterDownTimer = 0f;
		}

		public void ClearHudCenterDownText()
		{
			_hudCenterDownTime = -1f;
		}

		public void UpdateTimers()
		{
			if (_hudCenterUpTimer < _hudCenterUpTime)
				_hudCenterUpTimer += Time.deltaTime;
			else
				_hudCenterUpString = string.Empty;

			if (_hudCenterTimer < _hudCenterTime)
				_hudCenterTimer += Time.deltaTime;
			else
				_hudCenterString = string.Empty;

			if (_hudCenterDownTimer < _hudCenterDownTime)
				_hudCenterDownTimer += Time.deltaTime;
			else
				_hudCenterDownString = string.Empty;

			void UpdateExHud()
			{
				string curText = _hudTemplate.Replace("[STATS]",
					$"<color=#FF0000>K</color><color=#FF5500>o</color><color=#FFAA00>g</color><color=#FFFF00>n</color><color=#CCFF00>i</color><color=#99FF00>t</color><color=#66FF00>y</color><color=#33FF00>'</color><color=#00FF00>s</color><color=#00FF3F> </color><color=#00FF7F>P</color><color=#00FFBF>l</color><color=#00FFFF>a</color><color=#00CCFF>y</color><color=#0099FF>h</color><color=#0066FF>o</color><color=#0033FF>u</color><color=#0000FF>s</color><color=#3F00FF>e</color><color=#7F00FF> </color><color=#BF00FF>{Utils.ServerPort[Server.Port]}</color> [Server Time: {DateTime.Now:HH:mm:ss}]");

				// The top left "SCP LIST" thing
				//curText = curText.Replace("[LIST]", FormatStringForHud(string.Empty, 7));
			}
		}
	}
}