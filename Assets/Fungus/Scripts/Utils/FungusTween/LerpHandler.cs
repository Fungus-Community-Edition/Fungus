using UnityEngine;

namespace Fungus
{
    public delegate TReturnType LerpFunc<TReturnType>(TReturnType baseVal, TReturnType targetVal, float howFarAlong);
}