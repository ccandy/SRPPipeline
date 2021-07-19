using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LookOrFeel.Animation;

public class CurveGenerator : MonoBehaviour
{
    public AnimationCurve curve;    // for preview

    delegate double EasingFunction(double time,double min,double max,double duration);

    static AnimationCurve GenerateCurve(EasingFunction easingFunction, int resolution, bool IsRevert = false)
    {
        var curve = new AnimationCurve();

		if (IsRevert)
		{
			for (var i = 0; i < resolution; ++i)
			{
				var time = i / (resolution - 1f);
				var value = (float)easingFunction(1.0f - time, 0.0, 1.0, 1.0);
				var key = new Keyframe(time, value);
				curve.AddKey(key);
			}
		}
		else
		{
			for (var i = 0; i < resolution; ++i)
			{
				var time = i / (resolution - 1f);
				var value = (float)easingFunction(time, 0.0, 1.0, 1.0);
				var key = new Keyframe(time, value);
				curve.AddKey(key);
			}
		}
        for (var i = 0; i < resolution; ++i)
        {
            curve.SmoothTangents(i, 0f);
        }
        return curve;
    }

	static object GenerateParticleCurve(EasingFunction easingFunction, int resolution, bool IsRevert = false)
	{
		var DoubleCurveType = Type.GetType("UnityEditor.DoubleCurve, UnityEditor");

		

		var curve = new AnimationCurve();
		if (IsRevert)
		{
			for (var i = 0; i < resolution; ++i)
			{
				var time = i / (resolution - 1f);
				var value = (float)easingFunction(1.0f - time, 0.0, 1.0, 1.0);
				var key = new Keyframe(time, value);
				curve.AddKey(key);
			}
		}
		else
		{
			for (var i = 0; i < resolution; ++i)
			{
				var time = i / (resolution - 1f);
				var value = (float)easingFunction(time, 0.0, 1.0, 1.0);
				var key = new Keyframe(time, value);
				curve.AddKey(key);
			}
		}

		for (var i = 0; i < resolution; ++i)
		{
			curve.SmoothTangents(i, 0f);
		}

		var particleCurve = System.Activator.CreateInstance( DoubleCurveType, new object[] { null, curve, true });

		return particleCurve;
	}

	[MenuItem("Assets/Create/EasingCurves")]
    static void CreateAsset()
    {
        var curvePresetLibraryType = Type.GetType("UnityEditor.CurvePresetLibrary, UnityEditor");

		var particleCurvePresetLibraryType = Type.GetType("UnityEditor.DoubleCurvePresetLibrary, UnityEditor");

		var library = ScriptableObject.CreateInstance(curvePresetLibraryType);

		addCurve(library, PennerDoubleAnimation.Linear, 2, "Linear");

        addCurve(library, PennerDoubleAnimation.SineEaseIn, 15, "SineEaseIn");
        addCurve(library, PennerDoubleAnimation.QuadEaseIn, 15, "QuadEaseIn");
        addCurve(library, PennerDoubleAnimation.CubicEaseIn, 15, "CubicEaseIn");
        addCurve(library, PennerDoubleAnimation.QuartEaseIn, 15, "QuartEaseIn");
        addCurve(library, PennerDoubleAnimation.QuintEaseIn, 15, "QuintEaseIn");
        addCurve(library, PennerDoubleAnimation.ExpoEaseIn, 15, "ExpoEaseIn");
        addCurve(library, PennerDoubleAnimation.CircEaseIn, 15, "CircEaseIn");
        addCurve(library, PennerDoubleAnimation.BackEaseIn, 30, "BackEaseIn");
        addCurve(library, PennerDoubleAnimation.ElasticEaseIn, 30, "ElasticEaseIn");
        addCurve(library, PennerDoubleAnimation.BounceEaseIn, 30, "BounceEaseIn");

        addCurve(library, PennerDoubleAnimation.SineEaseOut, 15, "SineEaseOut");
        addCurve(library, PennerDoubleAnimation.QuadEaseOut, 15, "QuadEaseOut");
        addCurve(library, PennerDoubleAnimation.CubicEaseOut, 15, "CubicEaseOut");
        addCurve(library, PennerDoubleAnimation.QuartEaseOut, 15, "QuartEaseOut");
        addCurve(library, PennerDoubleAnimation.QuintEaseOut, 15, "QuintEaseOut");
        addCurve(library, PennerDoubleAnimation.ExpoEaseOut, 15, "ExpoEaseOut");
        addCurve(library, PennerDoubleAnimation.CircEaseOut, 15, "CircEaseOut");
        addCurve(library, PennerDoubleAnimation.BackEaseOut, 30, "BackEaseOut");
        addCurve(library, PennerDoubleAnimation.ElasticEaseOut, 30, "ElasticEaseOut");
        addCurve(library, PennerDoubleAnimation.BounceEaseOut, 30, "BounceEaseOut");

        addCurve(library, PennerDoubleAnimation.SineEaseInOut, 15, "SineEaseInOut");
        addCurve(library, PennerDoubleAnimation.QuadEaseInOut, 15, "QuadEaseInOut");
        addCurve(library, PennerDoubleAnimation.CubicEaseInOut, 15, "CubicEaseInOut");
        addCurve(library, PennerDoubleAnimation.QuartEaseInOut, 15, "QuartEaseInOut");
        addCurve(library, PennerDoubleAnimation.QuintEaseInOut, 15, "QuintEaseInOut");
        addCurve(library, PennerDoubleAnimation.ExpoEaseInOut, 15, "ExpoEaseInOut");
        addCurve(library, PennerDoubleAnimation.CircEaseInOut, 15, "CircEaseInOut");
        addCurve(library, PennerDoubleAnimation.BackEaseInOut, 30, "BackEaseInOut");
        addCurve(library, PennerDoubleAnimation.ElasticEaseInOut, 30, "ElasticEaseInOut");
        addCurve(library, PennerDoubleAnimation.BounceEaseInOut, 30, "BounceEaseInOut");

        addCurve(library, PennerDoubleAnimation.SineEaseOutIn, 15, "SineEaseOutIn");
        addCurve(library, PennerDoubleAnimation.QuadEaseOutIn, 15, "QuadEaseOutIn");
        addCurve(library, PennerDoubleAnimation.CubicEaseOutIn, 15, "CubicEaseOutIn");
        addCurve(library, PennerDoubleAnimation.QuartEaseOutIn, 15, "QuartEaseOutIn");
        addCurve(library, PennerDoubleAnimation.QuintEaseOutIn, 15, "QuintEaseOutIn");
        addCurve(library, PennerDoubleAnimation.ExpoEaseOutIn, 15, "ExpoEaseOutIn");
        addCurve(library, PennerDoubleAnimation.CircEaseOutIn, 15, "CircEaseOutIn");
        addCurve(library, PennerDoubleAnimation.BackEaseOutIn, 30, "BackEaseOutIn");
        addCurve(library, PennerDoubleAnimation.ElasticEaseOutIn, 30, "ElasticEaseOutIn");
        addCurve(library, PennerDoubleAnimation.BounceEaseOutIn, 30, "BounceEaseOutIn");

		var particleLibrary = ScriptableObject.CreateInstance(particleCurvePresetLibraryType);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.Linear, 2, "Linear");

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseIn, 15, "SineEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseIn, 15, "QuadEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseIn, 15, "CubicEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseIn, 15, "QuartEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseIn, 15, "QuintEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseIn, 15, "ExpoEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseIn, 15, "CircEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseIn, 30, "BackEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseIn, 30, "ElasticEaseIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseIn, 30, "BounceEaseIn");

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseOut, 15, "SineEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseOut, 15, "QuadEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseOut, 15, "CubicEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseOut, 15, "QuartEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseOut, 15, "QuintEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseOut, 15, "ExpoEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseOut, 15, "CircEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseOut, 30, "BackEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseOut, 30, "ElasticEaseOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseOut, 30, "BounceEaseOut");

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseInOut, 15, "SineEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseInOut, 15, "QuadEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseInOut, 15, "CubicEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseInOut, 15, "QuartEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseInOut, 15, "QuintEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseInOut, 15, "ExpoEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseInOut, 15, "CircEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseInOut, 30, "BackEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseInOut, 30, "ElasticEaseInOut");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseInOut, 30, "BounceEaseInOut");

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseOutIn, 15, "SineEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseOutIn, 15, "QuadEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseOutIn, 15, "CubicEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseOutIn, 15, "QuartEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseOutIn, 15, "QuintEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseOutIn, 15, "ExpoEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseOutIn, 15, "CircEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseOutIn, 30, "BackEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseOutIn, 30, "ElasticEaseOutIn");
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseOutIn, 30, "BounceEaseOutIn");


		if (!System.IO.Directory.Exists("Assets/Editor"))
		{
			System.IO.Directory.CreateDirectory("Assets/Editor");
		}

		AssetDatabase.CreateAsset(library, "Assets/Editor/EasingCurves.curves");

		var normalizedLibrary = Instantiate<ScriptableObject>(library);

		AssetDatabase.CreateAsset(normalizedLibrary, "Assets/Editor/EasingCurves.curvesNormalized");

		AssetDatabase.CreateAsset(particleLibrary, "Assets/Editor/EasingCurves.particleCurves");

		AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

		CreateRevertAsset();
	}

	static void CreateRevertAsset()
	{
		var curvePresetLibraryType = Type.GetType("UnityEditor.CurvePresetLibrary, UnityEditor");

		var particleCurvePresetLibraryType = Type.GetType("UnityEditor.DoubleCurvePresetLibrary, UnityEditor");

		var library = ScriptableObject.CreateInstance(curvePresetLibraryType);

		addCurve(library, PennerDoubleAnimation.Linear, 2, "LinearRevert", true);

		addCurve(library, PennerDoubleAnimation.SineEaseIn, 15, "SineEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuadEaseIn, 15, "QuadEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.CubicEaseIn, 15, "CubicEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuartEaseIn, 15, "QuartEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuintEaseIn, 15, "QuintEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.ExpoEaseIn, 15, "ExpoEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.CircEaseIn, 15, "CircEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.BackEaseIn, 30, "BackEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.ElasticEaseIn, 30, "ElasticEaseInRevert", true);
		addCurve(library, PennerDoubleAnimation.BounceEaseIn, 30, "BounceEaseInRevert", true);

		addCurve(library, PennerDoubleAnimation.SineEaseOut, 15, "SineEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuadEaseOut, 15, "QuadEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.CubicEaseOut, 15, "CubicEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuartEaseOut, 15, "QuartEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuintEaseOut, 15, "QuintEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.ExpoEaseOut, 15, "ExpoEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.CircEaseOut, 15, "CircEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.BackEaseOut, 30, "BackEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.ElasticEaseOut, 30, "ElasticEaseOutRevert", true);
		addCurve(library, PennerDoubleAnimation.BounceEaseOut, 30, "BounceEaseOutRevert", true);

		addCurve(library, PennerDoubleAnimation.SineEaseInOut, 15, "SineEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuadEaseInOut, 15, "QuadEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.CubicEaseInOut, 15, "CubicEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuartEaseInOut, 15, "QuartEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.QuintEaseInOut, 15, "QuintEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.ExpoEaseInOut, 15, "ExpoEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.CircEaseInOut, 15, "CircEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.BackEaseInOut, 30, "BackEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.ElasticEaseInOut, 30, "ElasticEaseInOutRevert", true);
		addCurve(library, PennerDoubleAnimation.BounceEaseInOut, 30, "BounceEaseInOutRevert", true);

		addCurve(library, PennerDoubleAnimation.SineEaseOutIn, 15, "SineEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuadEaseOutIn, 15, "QuadEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.CubicEaseOutIn, 15, "CubicEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuartEaseOutIn, 15, "QuartEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.QuintEaseOutIn, 15, "QuintEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.ExpoEaseOutIn, 15, "ExpoEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.CircEaseOutIn, 15, "CircEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.BackEaseOutIn, 30, "BackEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.ElasticEaseOutIn, 30, "ElasticEaseOutInRevert", true);
		addCurve(library, PennerDoubleAnimation.BounceEaseOutIn, 30, "BounceEaseOutInRevert", true);

		var particleLibrary = ScriptableObject.CreateInstance(particleCurvePresetLibraryType);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.Linear, 2, "LinearRevert", true);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseIn, 15, "SineEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseIn, 15, "QuadEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseIn, 15, "CubicEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseIn, 15, "QuartEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseIn, 15, "QuintEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseIn, 15, "ExpoEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseIn, 15, "CircEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseIn, 30, "BackEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseIn, 30, "ElasticEaseInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseIn, 30, "BounceEaseInRevert", true);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseOut, 15, "SineEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseOut, 15, "QuadEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseOut, 15, "CubicEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseOut, 15, "QuartEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseOut, 15, "QuintEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseOut, 15, "ExpoEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseOut, 15, "CircEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseOut, 30, "BackEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseOut, 30, "ElasticEaseOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseOut, 30, "BounceEaseOutRevert", true);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseInOut, 15, "SineEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseInOut, 15, "QuadEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseInOut, 15, "CubicEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseInOut, 15, "QuartEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseInOut, 15, "QuintEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseInOut, 15, "ExpoEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseInOut, 15, "CircEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseInOut, 30, "BackEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseInOut, 30, "ElasticEaseInOutRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseInOut, 30, "BounceEaseInOutRevert", true);

		addParticleCurve(particleLibrary, PennerDoubleAnimation.SineEaseOutIn, 15, "SineEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuadEaseOutIn, 15, "QuadEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CubicEaseOutIn, 15, "CubicEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuartEaseOutIn, 15, "QuartEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.QuintEaseOutIn, 15, "QuintEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ExpoEaseOutIn, 15, "ExpoEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.CircEaseOutIn, 15, "CircEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BackEaseOutIn, 30, "BackEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.ElasticEaseOutIn, 30, "ElasticEaseOutInRevert", true);
		addParticleCurve(particleLibrary, PennerDoubleAnimation.BounceEaseOutIn, 30, "BounceEaseOutInRevert", true);


		if (!System.IO.Directory.Exists("Assets/Editor"))
		{
			System.IO.Directory.CreateDirectory("Assets/Editor");
		}

		AssetDatabase.CreateAsset(library, "Assets/Editor/EasingCurvesRevert.curves");

		var normalizedLibrary = Instantiate<ScriptableObject>(library);

		AssetDatabase.CreateAsset(normalizedLibrary, "Assets/Editor/EasingCurvesRevert.curvesNormalized");

		AssetDatabase.CreateAsset(particleLibrary, "Assets/Editor/EasingCurvesRevert.particleCurves");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	static void addCurve(object library, EasingFunction easingFunction, int resolution, string name, bool IsRevert = false)
    {
        var curvePresetLibraryType = Type.GetType("UnityEditor.CurvePresetLibrary, UnityEditor");
        var addMehtod = curvePresetLibraryType.GetMethod("Add");
        addMehtod.Invoke(library, new object[]
        {
            GenerateCurve(easingFunction, resolution, IsRevert),
            name
        });
    }

	static void addParticleCurve(object library, EasingFunction easingFunction, int resolution, string name, bool IsRevert = false)
	{
		var curvePresetLibraryType = Type.GetType("UnityEditor.DoubleCurvePresetLibrary, UnityEditor");
		var addMehtod = curvePresetLibraryType.GetMethod("Add");
		addMehtod.Invoke(library, new object[]
		{
			GenerateParticleCurve(easingFunction, resolution, IsRevert),
			name
		});
	}
}
