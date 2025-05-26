using Hexa.NET.ImGui;
using IconFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Celesta
{
    public enum ImConverseNotice
    {
        Info,
        Warning,
        Error
    }
    public static partial class ImConverse
    {
        private static bool IsColorDark(Vector4 in_Color, float in_Tolerance = 255 / 2)
        {
            return (in_Color.X * 0.2126 + in_Color.Y * 0.7152 + in_Color.Z * 0.0722 < in_Tolerance);
        }
        public static void VerticalSeparator(float in_YSize = -1)
        {
            var pos = ImGui.GetCursorScreenPos();
            if (in_YSize == -1)
                in_YSize = ImGui.GetWindowSize().Y;
            var drawlist = ImGui.GetWindowDrawList();
            var p = ImGui.GetWindowPos();
            drawlist.AddLine(new Vector2(pos.X, p.Y), new Vector2(pos.X, p.Y + in_YSize), ImGui.GetColorU32(ImGuiCol.Border));
            ImGui.Dummy(new Vector2(5, p.Y));
        }
        /// <summary>
        /// Common class for both knobs
        /// </summary>
        /// <param name="in_Name"></param>
        /// <param name="value"></param>
        /// <param name="in_ActualKnobFunc"></param>
        /// <param name="in_Color"></param>
        /// <param name="in_KnobRadius"></param>
        /// <returns></returns>
        private static bool KnobWidgetBaseStart(string in_Name, ref int value, Func<Vector2, int> in_ActualKnobFunc, Vector4 in_Color, float in_KnobRadius = 40f)
        {
            //Let it be known, I split them after i made both.
            if (in_Color == default)
            {
                in_Color = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Border));
            }
            var colorBorders = ImGui.ColorConvertFloat4ToU32(in_Color);
            var drawList = ImGui.GetWindowDrawList();
            var io = ImGui.GetIO();
            var mousePos = io.MousePos;

            Vector2 cursorStart = ImGui.GetCursorScreenPos();
            Vector2 textSizeFake = ImGui.CalcTextSize(in_Name.PadRight(10, 'A'));
            Vector2 textSize = ImGui.CalcTextSize(in_Name);
            float fontSize = ImGui.GetFontSize();
            var textCol = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Text));


            //Make some padding based on text size, so it doesnt look like buttcheeks
            float verticalPadding = textSizeFake.Y * 0.5f;
            float horizontalPadding = textSizeFake.X * 0.25f;
            float knobDiameter = in_KnobRadius * 2;
            float inputHeight = fontSize + verticalPadding * 0.5f;

            //Calculate the entire widget's size
            float totalWidth = Math.Max(knobDiameter, textSizeFake.X) + horizontalPadding * 2;
            float totalHeight = textSizeFake.Y + verticalPadding + knobDiameter + verticalPadding + inputHeight + verticalPadding;

            Vector2 widgetTopLeft = cursorStart;
            Vector2 center = widgetTopLeft + new Vector2(totalWidth / 2f, textSizeFake.Y + verticalPadding + in_KnobRadius);

            //Rectangle around everything
            Vector2 widgetBottomRight = widgetTopLeft + new Vector2(totalWidth, totalHeight);
            bool isLightTheme = textCol.X <= 0.5f;
            if (isLightTheme)
            {
                drawList.AddRectFilled(widgetTopLeft, widgetBottomRight, colorBorders);
                if(IsColorDark(in_Color, 255 / 2))
                {
                    textCol = Vector4.One;
                }
            }
            else
            {
                drawList.AddRect(widgetTopLeft, widgetBottomRight, colorBorders);
            }

            //Title
            Vector2 labelPos = new Vector2(
                widgetTopLeft.X + (totalWidth - textSize.X) / 2f,
                widgetTopLeft.Y + verticalPadding / 2f
            );
            drawList.AddText(labelPos, ImGui.ColorConvertFloat4ToU32(textCol), in_Name);

            //Knob itself
            if (isLightTheme)
                drawList.AddCircleFilled(center, in_KnobRadius, ImGui.ColorConvertFloat4ToU32(textCol), 32);
            else
                drawList.AddCircle(center, in_KnobRadius, colorBorders, 32, 2.0f);

            in_ActualKnobFunc(center);

            //Inv button for spacing and to detect clicks
            ImGui.SetCursorScreenPos(widgetTopLeft);
            ImGui.InvisibleButton($"##{in_Name}_bttn", new Vector2(totalWidth, totalHeight - inputHeight - verticalPadding));
            bool isActive = ImGui.IsItemActive();

            //Input field below the knob
            Vector2 inputPos = new Vector2(
                widgetTopLeft.X + horizontalPadding,
                center.Y + in_KnobRadius + verticalPadding / 2f
            );
            ImGui.SetCursorScreenPos(inputPos);
            ImGui.SetNextItemWidth(totalWidth - horizontalPadding * 2);
            ImGui.InputInt($"##{in_Name}_inp", ref value, 0);
            ImGui.Dummy(new Vector2(totalWidth, verticalPadding));
            return isActive;
        }
        /// <summary>
        /// Provides an audio console-esque control for changing an integer.
        /// </summary>
        /// <param name="in_Name">Title of the control</param>
        /// <param name="id"></param>
        /// <param name="in_Percent"></param>
        /// <param name="in_KnobRadius"></param>
        /// <returns></returns>
        public static bool PercentKnob(string in_Name, ref int in_Percent, int in_Maximum, Vector4 in_Color = default, float in_KnobRadius = 40f)
        {
            if (in_Color == default)
            {
                in_Color = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Border));
            }
            var colorBorders = ImGui.ColorConvertFloat4ToU32(in_Color);
            ImGui.BeginGroup();
            var drawList = ImGui.GetWindowDrawList();
            var mousePos = ImGui.GetIO().MousePos;
            var angleMin = -135.0f;
            var angleMax = 135.0f;

            var multipliedPercent = Math.Clamp(in_Percent / (float)in_Maximum, 0, 1);
            var angle = angleMin + multipliedPercent * (angleMax - angleMin);
            Vector2 centerPoint = Vector2.Zero;
            var knob = KnobWidgetBaseStart(in_Name, ref in_Percent, (center) =>
            {
                centerPoint = center;
                float angleRad = angle * MathF.PI / 180f;
                Vector2 end = center + new Vector2(
                    MathF.Sin(angleRad) * (in_KnobRadius - (in_KnobRadius / 8)),
                    -MathF.Cos(angleRad) * (in_KnobRadius - (in_KnobRadius / 8))
                );
                drawList.AddLine(center, end, ImGui.GetColorU32(ImGuiCol.Text), 3.0f);
                return 0;
            }, in_Color);    

            bool returnValue = false;
            //If the knob is turned...
            if (knob && ImGui.IsMouseDown(0))
            {
                string angleText = $"{(int)(multipliedPercent * 100.0f)}%%";
                Vector2 wndSize = ImGui.CalcTextSize(angleText) * 1.2f;
                ImGui.SetNextWindowSize(wndSize);
                ImGui.SetNextWindowPos(centerPoint - wndSize / 2);

                //Window in the middle of the knob
                if (ImGui.Begin("##tooltipangle", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
                {
                    ImGui.Text(angleText);
                    ImGui.End();
                }

                Vector2 delta = mousePos - centerPoint;
                float newAngleRad = MathF.Atan2(delta.X, -delta.Y);
                int newAngleDeg = (int)(newAngleRad * 180f / MathF.PI);

                if (newAngleDeg < -135f)
                    newAngleDeg = -135;
                else if (newAngleDeg > 135f)
                    newAngleDeg = 135;

                if (newAngleDeg != in_Percent)
                {
                    in_Percent = (int)((newAngleDeg - angleMin) / (angleMax - angleMin) * (float)in_Maximum);
                    returnValue = true;
                }
            }
            // Reserve vertical space so the next item doesn't overlap
            ImGui.EndGroup();
            return returnValue;
        }
        private static Vector2 CalcAngleCircleVector(Vector2 in_Center, float in_CircleRadius, float angle)
        {
            float angleRad = angle * MathF.PI / 180f;
            return in_Center + new Vector2(
                MathF.Sin(angleRad) * (in_CircleRadius - (in_CircleRadius / 8)),
                -MathF.Cos(angleRad) * (in_CircleRadius - (in_CircleRadius / 8))
            );
        }
        /// <summary>
        /// Provides an audio console-esque control for changing an integer.
        /// </summary>
        /// <param name="in_Name">Title of the control</param>
        /// <param name="id"></param>
        /// <param name="in_Angle"></param>
        /// <param name="in_KnobRadius"></param>
        /// <returns></returns>
        public static bool AngleKnob(string in_Name, ref int in_Angle, Vector4 in_Color = default, float in_KnobRadius = 40f)
        {
            if(in_Color == default)
            {
                in_Color = ImGui.ColorConvertU32ToFloat4(ImGui.GetColorU32(ImGuiCol.Border));
            }
            var colorBorders = ImGui.ColorConvertFloat4ToU32(in_Color);
            ImGui.BeginGroup();
            var drawList = ImGui.GetWindowDrawList();
            var io = ImGui.GetIO();
            var mousePos = io.MousePos;
            Vector2 centerPoint = Vector2.Zero;
            var angleVar = in_Angle;
            var knob = KnobWidgetBaseStart(in_Name, ref in_Angle, (center) =>
            {
                centerPoint = center;
                float angleRad = angleVar * MathF.PI / 180f;
                Vector2 end = CalcAngleCircleVector(center, in_KnobRadius, angleVar);
                Vector2 angleForwardRight = CalcAngleCircleVector(center, in_KnobRadius, 20);
                Vector2 angleForwardLeft = CalcAngleCircleVector(center, in_KnobRadius, -20);
                Vector2 angleForward = CalcAngleCircleVector(center, in_KnobRadius, 0);
                Vector2 angleBackLeft = CalcAngleCircleVector(center, in_KnobRadius, -120);
                Vector2 angleBackRight = CalcAngleCircleVector(center, in_KnobRadius, 120);

                var color = ImGui.GetColorU32(ImGuiCol.Text, 0.05f);
                drawList.AddCircle(end, in_KnobRadius / 8, ImGui.GetColorU32(ImGuiCol.Text), 3.0f);
                drawList.AddLine(center, angleForwardLeft, color, 3.0f);
                drawList.AddLine(center, angleForwardRight, color, 3.0f);
                drawList.AddLine(center, angleForward, color, 3.0f);
                drawList.AddLine(center, angleBackLeft, color, 3.0f);
                drawList.AddLine(center, angleBackRight, color, 3.0f);
                return 0;
            }, in_Color);

            bool returnValue = false;
            //If the knob is turned...
            if (knob && ImGui.IsMouseDown(0))
            {
                string angleText = $"{in_Angle}°";
                Vector2 wndSize = ImGui.CalcTextSize(angleText) * 1.2f;
                ImGui.SetNextWindowSize(wndSize);
                ImGui.SetNextWindowPos(centerPoint - wndSize / 2);

                //Window in the middle of the knob
                if (ImGui.Begin("##tooltipangle", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize))
                {
                    ImGui.Text(angleText);
                    ImGui.End();
                }

                Vector2 delta = mousePos - centerPoint;
                float newAngleRad = MathF.Atan2(delta.X, -delta.Y);
                int newAngleDeg = (int)(newAngleRad * 180f / MathF.PI);

                if (newAngleDeg > 180) newAngleDeg -= 360;
                else if (newAngleDeg < -180) newAngleDeg += 360;

                if (newAngleDeg != in_Angle)
                {
                    in_Angle = newAngleDeg;
                    returnValue = true;
                }
            }
            ImGui.EndGroup();
            return returnValue;
        }
        public static void CenterWindow(Vector2 in_Size)
        {
            // Calculate centered position
            var viewport = ImGui.GetMainViewport();
            System.Numerics.Vector2 centerPos = new System.Numerics.Vector2(
                viewport.WorkPos.X + (viewport.WorkSize.X - in_Size.X) * 0.5f,
                viewport.WorkPos.Y + (viewport.WorkSize.Y - in_Size.Y) * 0.5f
            );
            ImGui.SetNextWindowPos(centerPos);
            ImGui.SetNextWindowSize(in_Size);
        }
        public static void EndListBoxCustom()
        {
            ImGui.EndGroup();
            ImGui.EndChild();
        }
        public static void ImageViewport(string in_Label, Vector2 in_Size, float in_ImageAspect, float in_Zoom, ImTextureID in_Texture, Action<SCenteredImageData> in_QuadDraw = null, Vector4 in_BackgroundColor = default)
        {
            float desiredSize = in_Size.X == -1 ? ImGui.GetContentRegionAvail().X : in_Size.X;
            var vwSize = new Vector2(desiredSize, desiredSize * in_ImageAspect);

            if (BeginListBoxCustom(in_Label, in_Size))
            {
                Vector2 cursorpos2 = ImGui.GetCursorScreenPos();
                var wndSize = ImGui.GetWindowSize();

                // Ensure viewport size correctly reflects the zoomed content
                var scaledSize = vwSize * in_Zoom;
                var vwPos = (wndSize - scaledSize) * 0.5f;

                var fixedVwPos = new Vector2(Math.Max(0, vwPos.X), Math.Max(0, vwPos.Y));

                // Set scroll region to match full zoomed element
                ImGui.SetCursorPosX(fixedVwPos.X);
                ImGui.SetCursorPosY(fixedVwPos.Y);

                if (in_BackgroundColor != Vector4.Zero)
                {
                    ImGui.AddRectFilled(ImGui.GetWindowDrawList(), ImGui.GetWindowPos() + fixedVwPos, ImGui.GetWindowPos() + fixedVwPos + scaledSize, ImGui.ColorConvertFloat4ToU32(in_BackgroundColor));

                }
                // Render the zoomed image
                ImGui.Image(
                    in_Texture, scaledSize,
                    new Vector2(0, 1), new Vector2(1, 0));
                in_QuadDraw?.Invoke(new SCenteredImageData(cursorpos2, ImGui.GetWindowPos(), scaledSize, fixedVwPos));
                //DrawQuadList(cursorpos2, windowPos, scaledSize, fixedVwPos);
            }
            EndListBoxCustom();
        }
        public static bool VisibilityNodeSimple(string in_Name, Action in_RightClickAction = null, bool in_ShowArrow = true, SIconData in_Icon = new(), string in_Id = "")
        {
            bool returnVal = true;
            bool idPresent = !string.IsNullOrEmpty(in_Id);
            string idName = idPresent ? in_Id : in_Name;
            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            var isLeaf = !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
            returnVal = ImGui.Selectable($"##{idName}header");
            ImGui.PopStyleVar();
            //Rightclick action
            if (in_RightClickAction != null)
            {
                if (ImGui.BeginPopupContextItem())
                {
                    in_RightClickAction.Invoke();
                    ImGui.EndPopup();
                }
            }
            //Visibility checkbox
            //ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //ImGui.Checkbox($"##{idName}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)

            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.SetNextItemAllowOverlap();

            ////Setup button so that the borders and background arent seen unless its hovered
            //ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            //ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            //ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            bool iconPresent = !in_Icon.IsNull();
            ////ImGui.Button($"##invButton{idName}", new Vector2(-1, 25));
            //ImGui.PopStyleColor(3);

            //Begin drawing text & icon if it exists
            ImGui.SetNextItemAllowOverlap();
            ImGui.PushID($"##text{idName}");
            ImGui.BeginGroup();

            if (iconPresent)
            {
                //Draw icon
                //ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0, 0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.TextColored(in_Icon.Color, in_Icon.Icon);
                //ImGui.PopFont();
                ImGui.SameLine(0, 0);
            }
            else
            {
                //Set size for the text as if there was an icon
                ImGui.SetCursorScreenPos(p + new Vector2(0, 2));
            }
            ImGui.SetNextItemAllowOverlap();
            ImGui.Text(iconPresent ? $" {in_Name}" : in_Name);

            ImGui.EndGroup();
            ImGui.PopID();
            return returnVal;
        }
        public static bool VisibilityNodeSimpleSlider(string in_Name, ref float in_Value, float in_Max, Action in_RightClickAction = null, SIconData in_Icon = new(), string in_Id = "")
        {
            bool returnVal = true;
            bool idPresent = !string.IsNullOrEmpty(in_Id);
            string idName = idPresent ? in_Id : in_Name;
            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            returnVal = ImGui.Button($"##{idName}header");
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            ImGui.SetNextItemAllowOverlap();
            ImGui.PopStyleVar();
            //Rightclick action
            if (in_RightClickAction != null)
            {
                if (ImGui.BeginPopupContextItem())
                {
                    in_RightClickAction.Invoke();
                    ImGui.EndPopup();
                }
            }
            //Visibility checkbox
            //ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //ImGui.Checkbox($"##{idName}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)

            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.SetNextItemAllowOverlap();

            ////Setup button so that the borders and background arent seen unless its hovered
            //ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            //ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            //ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            bool iconPresent = !in_Icon.IsNull();
            ////ImGui.Button($"##invButton{idName}", new Vector2(-1, 25));
            //ImGui.PopStyleColor(3);

            //Begin drawing text & icon if it exists
            ImGui.SetNextItemAllowOverlap();
            ImGui.PushID($"##text{idName}");
            ImGui.BeginGroup();

            if (iconPresent)
            {
                //Draw icon
                //ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0, 0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.TextColored(in_Icon.Color, in_Icon.Icon);
                //ImGui.PopFont();
                ImGui.SameLine(0, 0);
            }
            else
            {
                //Set size for the text as if there was an icon
                ImGui.SetCursorScreenPos(p + new Vector2(0, 2));
            }
            ImGui.SetNextItemAllowOverlap();
            ImGui.Text(iconPresent ? $" {in_Name}" : in_Name);
            ImGui.SameLine(0, 0);
            ImGui.SetNextItemAllowOverlap();
            ImGui.SliderFloat("##sliderrr", ref in_Value, 0, 1);

            ImGui.EndGroup();
            ImGui.PopID();
            return returnVal;
        }
        public static bool VisibilityNode(string in_Name, ref bool in_IsSelected, Action in_RightClickAction = null, bool in_ShowArrow = true, SIconData in_Icon = new(), string in_Id = "")
        {
            bool returnVal = true;
            bool idPresent = !string.IsNullOrEmpty(in_Id);
            string idName = idPresent ? in_Id : in_Name;
            //Make header fit the content
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 3));
            var isLeaf = !in_ShowArrow ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None;
            returnVal = ImGui.TreeNodeEx($"##{idName}header", isLeaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
            ImGui.PopStyleVar();
            //Rightclick action
            if (in_RightClickAction != null)
            {
                if (ImGui.BeginPopupContextItem())
                {
                    in_RightClickAction.Invoke();
                    ImGui.EndPopup();
                }
            }
            //Visibility checkbox
            //ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //ImGui.Checkbox($"##{idName}togg", ref in_Visibile);
            ImGui.SameLine(0, 1 * ImGui.GetStyle().ItemSpacing.X);
            //Show text with icon (cant have them merged because of stupid imgui c# bindings)

            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.SetNextItemAllowOverlap();

            //Setup button so that the borders and background arent seen unless its hovered
            ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0)));
            bool iconPresent = !in_Icon.IsNull();
            in_IsSelected = ImGui.Button($"##invButton{idName}", new Vector2(-1, 25));
            ImGui.PopStyleColor(3);

            //Begin drawing text & icon if it exists
            ImGui.SetNextItemAllowOverlap();
            ImGui.PushID($"##text{idName}");
            ImGui.BeginGroup();

            if (iconPresent)
            {
                //Draw icon
                //ImGui.PushFont(ImGuiController.FontAwesomeFont);
                ImGui.SameLine(0, 0);
                ImGui.SetNextItemAllowOverlap();
                ImGui.SetCursorScreenPos(p);
                ImGui.TextColored(in_Icon.Color, in_Icon.Icon);
                //ImGui.PopFont();
                ImGui.SameLine(0, 0);
            }
            else
            {
                //Set size for the text as if there was an icon
                ImGui.SetCursorScreenPos(p + new Vector2(0, 2));
            }
            ImGui.SetNextItemAllowOverlap();
            ImGui.Text(iconPresent ? $" {in_Name}" : in_Name);

            ImGui.EndGroup();
            ImGui.PopID();
            return returnVal;
        }
        /// <summary>
        /// Fake list box that allows horizontal scrolling
        /// </summary>
        /// <param name="in_Label"></param>
        /// <param name="in_Size"></param>
        /// <returns></returns>
        public static bool BeginListBoxCustom(string in_Label, Vector2 in_Size)
        {
            bool returnVal = ImGui.BeginChild(in_Label, in_Size, ImGuiChildFlags.FrameStyle, ImGuiWindowFlags.HorizontalScrollbar);
            unsafe
            {
                //Ass Inc.
                //This is so that the child window has the same color as normal list boxes would
                ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.FrameBg)));
            }
            ImGui.BeginGroup();
            ImGui.PopStyleColor();
            return returnVal;
        }

        internal static void Notice(string v, ImConverseNotice warning)
        {
            string icon = FontAwesome6.CircleInfo;
            Vector4 color = Vector4.Zero;
            switch (warning)
            {
                case ImConverseNotice.Info:
                    color = ColorResource.Celeste;
                    icon = FontAwesome6.CircleInfo;
                    break;
                case ImConverseNotice.Warning:
                    color = new Vector4(1, 0.8f, 0, 1);
                    icon = FontAwesome6.TriangleExclamation;
                    break;
                case ImConverseNotice.Error:
                    color = new Vector4(1, 0.3f, 0, 1);
                    icon = FontAwesome6.CircleExclamation;
                    break;
            }
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.ColorConvertFloat4ToU32(color));
            if (ImGui.BeginListBox($"##warning_{v}", new Vector2(-1, ImGui.GetFontSize() * 3.5f)))
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(ImGui.GetFontSize() * 0.8f, ImGui.GetFontSize()));

                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(color));
                ImGui.Text(icon);
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.Text(v);
                ImGui.EndListBox();
            }
            ImGui.PopStyleColor();
        }
    }
}
