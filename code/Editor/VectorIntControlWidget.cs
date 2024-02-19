using Editor;
using Minesweeper.Mth;
using Sandbox;

namespace Minesweeper.Editor;

[CustomEditor(typeof(Vector3Int))]
[CustomEditor(typeof(Vector2Int))]
public class VectorIntControlWidget : VectorControlWidget
{
    public VectorIntControlWidget(SerializedProperty property) : base(property)
    {

    }
}
