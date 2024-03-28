using Editor;
using Minesweeper.Mth;
using Sandbox;

namespace Minesweeper.Editor;

[CustomEditor(typeof(Vector3IntB))]
[CustomEditor(typeof(Vector2IntB))]
public class VectorIntControlWidget : VectorControlWidget
{
    public VectorIntControlWidget(SerializedProperty property) : base(property)
    {

    }
}
