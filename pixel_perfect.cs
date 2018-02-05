using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode;
using ReflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage;
using LightProbeUsage = UnityEngine.Rendering.LightProbeUsage;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PixelPerfectDisplay : MonoBehaviour
{
  public int res_w = 640;
  public int res_h = 480;
  public int pixels_per_unit = 100;
  public int scale_factor = 4;

  private Camera input_camera;
  private RenderTexture render_texture;
  private RenderTexture render_texture_scaled;
  private RenderTexture target_texture;

  private int screen_width;
  private int screen_height;

  private Rect window_rect;

  void adjust_viewport()
  {
    float screen_aspect = (float) Screen.width / (float) Screen.height;
    float target_aspect = (float)res_w / (float)res_h;

    window_rect = new Rect();
    if (screen_aspect > target_aspect)
    {
      window_rect.width = target_aspect / screen_aspect;
      window_rect.height = 1.0f;
      window_rect.x = 0.5f - (window_rect.width / 2);
      window_rect.y = 0.0f;
    }
    else
    {
      window_rect.width = 1.0f;
      window_rect.height = (1 / target_aspect) / (1 / screen_aspect);
      window_rect.x = 0.0f;
      window_rect.y = 0.5f - (window_rect.height / 2);
    }

  }

  void setup_camera()
  {
    input_camera.orthographic = true;
    input_camera.aspect = (float)res_w / (float)res_h;
    input_camera.orthographicSize = (float)res_h / (float)(pixels_per_unit * 2);
    input_camera.nearClipPlane = -0.5f;
    input_camera.farClipPlane = 0.5f;
  }

  // Use this for initialization
  void Awake()
  {
    screen_width = Screen.width;
    screen_height = Screen.height;

    // Create rendertexture for input camera
    render_texture = new RenderTexture(res_w, res_h, 0, RenderTextureFormat.ARGB32);
    render_texture.filterMode = FilterMode.Point;
    render_texture.Create();

    render_texture_scaled = new RenderTexture(scale_factor * res_w, scale_factor * res_h, 0, RenderTextureFormat.ARGB32);
    render_texture_scaled.filterMode = FilterMode.Bilinear;
    render_texture_scaled.Create();

    // Set up input camera
    input_camera = GetComponent<Camera>();
    setup_camera();

    adjust_viewport();
  }

  void Update()
  {
    if (Application.isEditor)
      setup_camera();

    if (screen_width != Screen.width || screen_height != Screen.height)
      adjust_viewport();
  }

  void OnDestroy()
  {
    render_texture.Release();
    render_texture_scaled.Release();
  }

  void OnPreRender()
  {
    input_camera.rect = new Rect(0, 0, 1, 1);
    input_camera.targetTexture = render_texture;
  }

  void OnRenderImage(RenderTexture in_tex, RenderTexture out_tex)
  {
    in_tex.filterMode = FilterMode.Point;
    Graphics.Blit(in_tex, render_texture_scaled);
    Graphics.Blit(render_texture_scaled, out_tex);
  }

  void OnPostRender()
  {
    input_camera.rect = window_rect;
    input_camera.targetTexture = null;
  }
}

