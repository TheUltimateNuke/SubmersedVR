using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.XR;

namespace SubmersedVR
{

    // This replaces the main camera transform in `Targeting` with the event camera from `VRCameraRig`
    // TODO: Rewrite with CodeMatcher
    [HarmonyPatch(typeof(Targeting), nameof(Targeting.GetTarget))]
    [HarmonyPatch([typeof(float), typeof(GameObject), typeof(float)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out])]
    public static class WorldTargetingWithController
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ins in instructions)
            {
                bool skipNext = false;
                // Look for `Transform transform = MainCamera.camera.transform;` in the first line of the method and replace it with our own
                if (ins.Calls(typeof(MainCamera).GetProperty(nameof(MainCamera.camera)).GetGetMethod()))
                {
                    // Skip next instruction which called/got transform from the main camera
                    skipNext = true;
                    MethodInfo targetTransformGetter = typeof(VRCameraRig).GetMethod(nameof(VRCameraRig.GetTargetTansform));
                    yield return new CodeInstruction(OpCodes.Call, targetTransformGetter);
                }
                else if (skipNext)
                {
                }
                else
                    yield return ins;
            }
        }
    }

    // TODO: Reorganize/Move patches
    // Remove/Disable the movement of the HandReticle in LateUpdate by rewriting the if(XRSettings.enabled) to if(false)
    [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
    public static class NoReticleMovementInVR
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ins in instructions)
            {
                // Instead of the call to the enabled property we just push 0/false on to the stack to skip the if
                yield return ins.Calls(AccessTools.DeclaredPropertyGetter(typeof(XRSettings), nameof(XRSettings.enabled)))
                    ? new CodeInstruction(OpCodes.Ldc_I4_0)
                    : ins;
            }
        }
    }

    // Sets the default DesiredIcon in LateUpdate to None(0) instead of Default(1)
    [HarmonyPatch(typeof(HandReticle), nameof(HandReticle.LateUpdate))]
    public static class NoDefaultReticleIcon
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo desiredIconField = AccessTools.Field(typeof(HandReticle), nameof(HandReticle.desiredIconType));
            return new CodeMatcher(instructions).MatchForward(false, [
                // /* 0x0012B387 17           */ IL_015F: ldc.i4.1
                // /* 0x0012B388 7DB5320004   */ IL_0160: stfld     valuetype HandReticle/IconType HandReticle::desiredIconType
                new(OpCodes.Ldc_I4_1), // Store 1
                new(opc => opc.StoresField(desiredIconField)),
            ]).SetOpcodeAndAdvance(OpCodes.Ldc_I4_0).InstructionEnumeration(); // Replace by 0
        }
    }

}