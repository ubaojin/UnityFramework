using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Input Nodes")]
		[Serializable]
		public class TransformInputNode : InputNode<Transform>
		{
		}
	}
}