using UnityEngine;
using System.Collections;

public class TargetInfo : MonoBehaviour {
	public enum TargetType {None, Friendly, Enemy}
	public TargetType targetType = TargetType.None;
	public string targetName = "Target";
}