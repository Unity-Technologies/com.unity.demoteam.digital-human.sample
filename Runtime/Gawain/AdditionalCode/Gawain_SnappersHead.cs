using UnityEngine;
using Unity.DemoTeam.DigitalHuman;
using Unity.Collections.LowLevel.Unsafe;// for UnsafeUtilityEx.AsRef<T>

namespace Unity.DemoTeam.DigitalHuman.Sample
{
	using SnappersControllers = Gawain_SnappersControllers<SnappersController>;
	using SnappersBlendShapes = Gawain_SnappersBlendShapes<float>;

	public class Gawain_SnappersHead : SnappersHeadDefinition
	{
		public override InstanceData CreateInstanceData(Mesh sourceMesh, Transform sourceRig, Warnings warnings)
		{
			return CreateInstanceData<SnappersControllers, SnappersBlendShapes>(sourceMesh, sourceRig, warnings);
		}

		bool CheckSizes()
		{
			const int INDEXED_SIZE_CONTROLLERS = 5440;
			const int INDEXED_SIZE_BLENDSHAPES = 1288;
			const int INDEXED_SIZE_SHADERPARAM = 540;

			return
				INDEXED_SIZE_CONTROLLERS <= UnsafeUtility.SizeOf<SnappersControllers>() &&
				INDEXED_SIZE_BLENDSHAPES <= UnsafeUtility.SizeOf<SnappersBlendShapes>() &&
				INDEXED_SIZE_SHADERPARAM <= UnsafeUtility.SizeOf<SnappersShaderParam>();
		}

		public override unsafe void ResolveControllers(void* ptrSnappersControllers, void* ptrSnappersBlendShapes, void* ptrSnappersShaderParam)
		{
			if (!CheckSizes())
				return;

			Gawain_SnappersHeadImpl.ResolveControllers(
				(float*)ptrSnappersControllers,
				(float*)ptrSnappersBlendShapes,
				(float*)ptrSnappersShaderParam
			);
		}

		public override unsafe void ResolveBlendShapes(void* ptrSnappersControllers, void* ptrSnappersBlendShapes, void* ptrSnappersShaderParam)
		{
			if (!CheckSizes())
				return;

			Gawain_SnappersHeadImpl.ResolveBlendShapes(
				(float*)ptrSnappersControllers,
				(float*)ptrSnappersBlendShapes,
				(float*)ptrSnappersShaderParam
			);
		}

		public override unsafe void ResolveShaderParam(void* ptrSnappersControllers, void* ptrSnappersBlendShapes, void* ptrSnappersShaderParam)
		{
			if (!CheckSizes())
				return;

			Gawain_SnappersHeadImpl.ResolveShaderParam(
				(float*)ptrSnappersControllers,
				(float*)ptrSnappersBlendShapes,
				(float*)ptrSnappersShaderParam
			);
		}

		public override unsafe void InitializeControllerCaps(void* ptrSnappersControllers)
		{
			Gawain_SnappersHeadImpl.InitializeControllerCaps(
				(uint*)ptrSnappersControllers
			);
		}
	}
}
