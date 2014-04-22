using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
using System.Reflection;
using System.Collections.Generic;

namespace DeckSearch
{
	public class DeckSearch : BaseMod
	{
		GUIStyle searchFieldStyle;

		List<DeckInfo> fullDeckList;

		private string searchFieldString = string.Empty;

		Popups popups;

		Rect rect;

		//initialize everything here, Game is loaded at this point
		public DeckSearch ()
		{
			this.searchFieldStyle = ((GUISkin)ResourceManager.Load ("_GUISkins/TextEntrySkin")).textField;
		}


		public static string GetName ()
		{
			return "DeckSearch";
		}

		public static int GetVersion ()
		{
			return 1;
		}

		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
			MethodDefinition[] definitions;
			definitions = scrollsTypes["Popups"].Methods.GetMethod ("ShowDeckSelector");
			MethodDefinition show = definitions[0];
			definitions = scrollsTypes["Popups"].Methods.GetMethod ("DrawDeckSelector");
			MethodDefinition draw = definitions[0];
			return new MethodDefinition[] {show, draw};
		}

		
		public override void BeforeInvoke (InvocationInfo info)
		{
			return;
		}

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
			this.popups = (Popups)info.target;
			if (info.targetMethod == "ShowDeckSelector") {
				this.fullDeckList = (List<DeckInfo>)typeof(Popups).GetField ("deckList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this.popups);
				return;
			}
			if (info.targetMethod == "DrawDeckSelector") {
				this.rect = (Rect)info.arguments[0];
				float headerHeight = (float)Screen.height * 0.055f;
				Rect textRect = new Rect (rect.x, rect.y, rect.width * 0.3f, headerHeight);

				int orig = GUI.skin.textField.fontSize;
				string text = this.searchFieldString;
				this.searchFieldString = GUI.TextField (textRect, this.searchFieldString, this.searchFieldStyle);
				if (this.searchFieldString != text) {
					this.filter();
				}
				return;
			}
			return;
		}

		public void filter()
		{
			// TODO: support r:g for resource:growth, etc.
			String search = this.searchFieldString.ToLower ();
			List<DeckInfo> partialList = fullDeckList.FindAll ((DeckInfo i) => (i.name.ToLower ().Contains (search)));
			this.popups.UpdateDecks(partialList);
			MethodInfo method = typeof(Popups).GetMethod ("DrawDeckSelector", BindingFlags.Instance | BindingFlags.NonPublic);
			object[] parameters = new object[] { this.rect };
			method.Invoke(this.popups, parameters);
		}
	}
}

