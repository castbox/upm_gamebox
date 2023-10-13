using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameBox
{
	public class TextGradientColor : BaseMeshEffect
	{
		public static List<UIVertex> helpList = new List<UIVertex>();

		[SerializeField]
		private Color32 topColor = Color.yellow;
		[SerializeField]
		private Color32 bottomColor = Color.red;

		private void ModifyMeshText(VertexHelper vertexHelper)
		{
			helpList.Clear();
			vertexHelper.GetUIVertexStream(helpList);
			int count = helpList.Count;
			if (count > 0)
			{
				for (int i = 0; i < helpList.Count;)
				{
					float bottomY = helpList[i].position.y;
					float topY = bottomY;
					float dis = 1f;
					for (int k = 1; k < 6; k++)
					{
						float y = helpList[k + i].position.y;
						if (y > topY)
						{
							topY = y;
						}
						else if (y < bottomY)
						{
							bottomY = y;
						}
					}
					dis = topY - bottomY;
					for (int k = 0; k < 6; k++)
					{
						UIVertex vertText = helpList[k + i];
						vertText.color = Color32.Lerp(bottomColor, topColor, (vertText.position.y - bottomY) / dis);
						helpList[k + i] = vertText;
					}
					i += 6;
				}
				vertexHelper.Clear();
				vertexHelper.AddUIVertexTriangleStream(helpList);
				helpList.Clear();
			}
		}

		public override void ModifyMesh(VertexHelper vertexHelper)
		{
			if (!IsActive())
				return;

			ModifyMeshText(vertexHelper);
		}
	}
}
