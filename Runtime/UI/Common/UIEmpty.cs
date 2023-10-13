using UnityEngine.UI;
namespace GameBox
{
    /// <summary>
    /// 无绘制的接受点击
    /// </summary>
    public class UIEmpty : Graphic
    {
        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
