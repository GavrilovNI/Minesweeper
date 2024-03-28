using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Mth;

public static class Vector3Extensions
{
    public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal) => Vector3.VectorPlaneProject(vector, planeNormal);
}
