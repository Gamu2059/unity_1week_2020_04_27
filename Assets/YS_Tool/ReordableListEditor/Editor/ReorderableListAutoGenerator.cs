using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ReorderableListAutoGenerator
{
	const string EDITOR_NAME = "Editor";
	const string REPLACE_STRING = "TARGET_CLASS_NAME";
	const string TEMPLATE_FILE = "Assets/YS_Tool/ReordableListEditor/Editor/ReorderableListEditorTemplate.txt";

	[MenuItem( "Assets/Generate ReordableListEditor" )]
	public static void ExportReorderableListEditor()
	{
		foreach( var obj in Selection.objects )
		{
			if( obj is MonoScript )
			{
				var path = AssetDatabase.GetAssetPath( obj );
				int _backslash = path.LastIndexOf( "\\" );
				int _slash = path.LastIndexOf( "/" );
				var parentPath = path.Substring( 0, Mathf.Max( _backslash, _slash ) );
				var editorPath = Path.Combine( parentPath, EDITOR_NAME );
				Directory.CreateDirectory( editorPath );

				string template = File.ReadAllText( TEMPLATE_FILE );
				string script = template.Replace( REPLACE_STRING, obj.name );
				File.WriteAllText( Path.Combine( editorPath, obj.name + "Editor.cs" ), script );
			}
		}
	}

}
