using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatToggle : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<bool> _toggle = false;
			public float _speed = 1.0f;
			#endregion

			#region Private Data 
			private float _value;
			#endregion

			#region Node
			public override void Init()
			{
				_value = 0.0f;
			}

			public override void Update(float deltaTime)
			{
				if (_toggle)
				{
					_value = Mathf.Min(_value + _speed * deltaTime, 1.0f);
				}
				else
				{
					_value = Mathf.Max(_value - _speed * deltaTime, 0.0f);
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return _value;
			}
			#endregion
		}
	}
}