using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
#endif

namespace MPX.Tools
{
#if UNITY_EDITOR
    public class GizmosDrawer : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            DebugGizmos.DrawAllGizmos();
        }

        private void Update()
        {
            DebugGizmos.ExternalTime = Time.realtimeSinceStartup;
        }
    }
    
    public class GizmoSelect : MonoBehaviour
    {
        public long SelectionId;
    }
#endif


    public static class DebugGizmos
    {
#if UNITY_EDITOR
        #region Types
        public enum DrawMode
        {
            Disabled,
            Selected,
            Allways
        }

        public class DrawModeInfo
        {
            public DrawModeInfo parent = null;
            public DrawMode mode;

            public DrawMode GetMode()
            {
                if (mode == DrawMode.Disabled) return DrawMode.Disabled;
                if (parent == null) return mode;

                var parentMode = parent.GetMode();
                return (int) parentMode < (int) mode ? parentMode : mode;
            }
        }

        class GizmoCommandEntry
        {
            public float LastUpdateTime;
            public GizmoCommand Command;
        }
        #endregion

        #region Commands
        class GizmoCommand
        {
            public string Category;
            public long Id;
            public Vector3 Position;
            public Color Color;
            public float RemainTime;

            public virtual void Draw()
            {
            }
        }

        class LineCommand : GizmoCommand
        {
            public Vector3 End;
            public float DotSize;

            public override void Draw()
            {
                base.Draw();

                Handles.color = Color;

                if (DotSize <= 0)
                    Handles.DrawLine(Position, End);
                else
                    Handles.DrawDottedLine(Position, End, DotSize);
            }
        }
        
        class PolyLineCommand : GizmoCommand
        {
            public Vector3[] Points;
            public float DotSize;

            public override void Draw()
            {
                base.Draw();

                Handles.color = Color;

                for (var i = 0; i < Points.Length - 1; i++)
                {
                    var p1 = Points[i];
                    var p2 = Points[i + 1];
                    
                    if (DotSize <= 0)
                        Handles.DrawLine(p1, p2);
                    else
                        Handles.DrawDottedLine(p1, p2, DotSize);
                }
            }
        }

        class InterruptedLineCommand : GizmoCommand
        {
            public Vector3 InterruptPos;
            public Vector3 End;
            public float DotSize;

            public override void Draw()
            {
                base.Draw();

                Handles.color = Color;

                Handles.DrawLine(Position, InterruptPos);

                if (DotSize <= 0)
                    Handles.DrawLine(InterruptPos, End);
                else
                    Handles.DrawDottedLine(InterruptPos, End, DotSize);
            }
        }

        class ArrowCommand : LineCommand
        {
            public float HeadSize;

            public override void Draw()
            {
                base.Draw();

                var dir = (End - Position).normalized;
                
                if (dir.sqrMagnitude > 0)
                    Handles.ConeHandleCap(0, End - dir * HeadSize * 0.5f, Quaternion.LookRotation(dir), HeadSize, EventType.Repaint);
            }
        }

        class InterruptedArrowCommand : InterruptedLineCommand
        {
            public float HeadSize;

            public override void Draw()
            {
                base.Draw();
                
                var dir = (InterruptPos - Position).normalized;
                
                if (dir.sqrMagnitude > 0)
                    Handles.ConeHandleCap(0, InterruptPos - dir * HeadSize * 0.5f, Quaternion.LookRotation(dir), HeadSize, EventType.Repaint);
            }
        }

        class TextCommand : GizmoCommand
        {
            public string Text;
            public int FontSize;
            public float RiseSpeed = 0;

            public override void Draw()
            {
                base.Draw();

                var textContent = new GUIContent(Text);

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color;
                if (FontSize > 0)
                    style.fontSize = FontSize;

                Vector2 textSize = style.CalcSize(textContent);
                Vector3 screenPoint = Camera.current.WorldToScreenPoint(Position);

                if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
                {
                    var worldPosition = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f, screenPoint.z));
                    Handles.Label(worldPosition, textContent, style);
                }

                Position += Vector3.up * RiseSpeed * Time.deltaTime;
            }
        }

        class DiscCommand : GizmoCommand
        {
            public bool Solid;
            public Vector3 Normal;
            public float Radius;

            public override void Draw()
            {
                base.Draw();
                
                Handles.color = Color;
                
                if (Solid)
                    Handles.DrawSolidDisc(Position, Normal, Radius);
                else
                    Handles.DrawWireDisc(Position, Normal, Radius);
            }
        }
        
        class SphereCommand : GizmoCommand
        {
            public bool Solid;
            public float Radius;

            public override void Draw()
            {
                base.Draw();
                
                Gizmos.color = Color;
                
                if (Solid)
                    Gizmos.DrawSphere(Position, Radius);
                else
                    Gizmos.DrawWireSphere(Position, Radius);
            }
        }
        
        class ConeCommand : GizmoCommand
        {
            public float Angle;
            public Vector3 End;
            public float Period;
            public bool DrawLines;

            public override void Draw()
            {
                base.Draw();

                var delta = End - Position;
                float len = delta.magnitude;
                var dir = delta.normalized;

                float angSin = Mathf.Sin(Mathf.Deg2Rad * Angle);
                float radius = angSin * len;
                
                Handles.color = Color;

                if (Period > 0)
                {
                    for (float dist = Period; dist < len; dist += Period)
                    {
                        Handles.DrawWireDisc(Position + dir * dist, dir, angSin * dist);
                    }
                }
                
                Handles.DrawWireDisc(End, dir, radius);

                if (DrawLines)
                {
                    var perpendicular1 = Vector3.Cross(dir, Vector3.up).normalized;
                    var perp2d = Vector2.Perpendicular(new Vector2(dir.x, dir.z));
                    var perpendicular2 = Vector3.Cross(dir, new Vector3(perp2d.x, 0, perp2d.y)).normalized;
                    
                    Handles.DrawLine(Position, End + perpendicular1 * radius);
                    Handles.DrawLine(Position, End - perpendicular1 * radius);
                    Handles.DrawLine(Position, End + perpendicular2 * radius);
                    Handles.DrawLine(Position, End - perpendicular2 * radius);
                }
            }
        }

        class SectorCommand : GizmoCommand
        {
            public float Angle;
            public Vector3 End;

            public override void Draw()
            {
                base.Draw();
                
                var delta = End - Position;
                float len = delta.magnitude;
                var dir = delta.normalized;

                float angSin = Mathf.Sin(Mathf.Deg2Rad * Angle);
                float radius = angSin * len;
                
                Handles.color = Color;
                
                var perp2d = Vector2.Perpendicular(new Vector2(dir.x, dir.z));
                var perpendicular = Vector3.Cross(dir, new Vector3(perp2d.x, 0, perp2d.y)).normalized;

                var start = Quaternion.AngleAxis(-Angle * 0.5f, perpendicular) * dir;
                var end = Quaternion.AngleAxis(Angle * 0.5f, perpendicular) * dir;

                Handles.DrawWireArc(Position, perpendicular, start, Angle, len);
                Handles.DrawLine(Position, Position + start * len);
                Handles.DrawLine(Position, Position + end * len);
            }
        }
        
        class BoxCommand : GizmoCommand
        {
            public bool Solid;
            public Vector3 Size;
            public Quaternion Rotation;

            public override void Draw()
            {
                base.Draw();
                
                Gizmos.color = Color;
                
                if (Solid)
                    Gizmos.DrawCube(Position, Size);
                else
                    Gizmos.DrawWireCube(Position, Size);
            }
        }
        #endregion
        


        #region Internal
        const float _clearTime = 0.5f;
        static List<GizmoCommand> _commands = new List<GizmoCommand>();
        static List<GizmoCommandEntry> _updatedGizmos = new List<GizmoCommandEntry>();


        public static bool Enabled = false;
        public static readonly Dictionary<string, DrawModeInfo> CategoriesSetup = new Dictionary<string, DrawModeInfo>();
        public static readonly HashSet<long> SelectedIds = new HashSet<long>();

        public static float ExternalTime = 0f;

        public static void SetCategoryMode(string category, DrawMode mode)
        {
            category = category.TrimEnd('/');

            if (CategoriesSetup.TryGetValue(category, out var modeInfo))
            {
                modeInfo.mode = mode;
            }
            else
            {
                AddCategoryInfo(category).mode = mode;
            }
        }

        public static DrawMode GetCategoryMode(string category)
        {
            category = category.TrimEnd('/');

            if (!CategoriesSetup.TryGetValue(category, out var modeInfo))
            {
                modeInfo = AddCategoryInfo(category);
            }

            return modeInfo.GetMode();
        }

        private static DrawModeInfo AddCategoryInfo(string category)
        {
            var subCategories = category.Split('/');

            DrawModeInfo catParent = null;
            string path = "";

            bool rootCategory = true;
            foreach (var subCategory in subCategories)
            {
                path = path + (rootCategory ? "" : "/") + subCategory;
                if (!CategoriesSetup.TryGetValue(path, out var modeInfo))
                {
                    modeInfo = new DrawModeInfo()
                    {
                        mode = DrawMode.Allways,
                        parent = catParent
                    };

                    CategoriesSetup[path] = modeInfo;
                }

                catParent = modeInfo;
                rootCategory = false;
            }

            return catParent;
        }

        public static void DrawAllGizmos()
        {
            if (!Enabled) return;

            float dt = Time.deltaTime;

            lock (_commands)
            {
                foreach (var command in _commands)
                {
                    command.RemainTime -= dt;

                    var mode = GetCategoryMode(command.Category);

                    if (mode == DrawMode.Disabled) continue;
                    if (mode == DrawMode.Selected && !SelectedIds.Contains(command.Id)) continue;

                    command.Draw();
                }

                _commands.RemoveAll(c => c.RemainTime <= 0);
            }

            lock (_updatedGizmos)
            {
                foreach (var entry in _updatedGizmos)
                {
                    var command = entry.Command;
                    var mode = GetCategoryMode(command.Category);

                    if (mode == DrawMode.Disabled) continue;
                    if (mode == DrawMode.Selected && !SelectedIds.Contains(command.Id)) continue;

                    command.Draw();
                }


                _updatedGizmos.RemoveAll(e => (ExternalTime - e.LastUpdateTime) > _clearTime);
            }
        }

        static void AddCommand(GizmoCommand command)
        {
            if (!Enabled) return;

            if (string.IsNullOrEmpty(command.Category)) return;

            lock (_commands)
            {
                _commands.Add(command);
            }
        }

        static T GetCommand<T>(string category, long id) where T : GizmoCommand, new()
        {
            if (!Enabled) return null;

            if (string.IsNullOrEmpty(category)) return null;

            lock (_updatedGizmos)
            {
                var entry = _updatedGizmos.Find(ce => ce.Command.Id == id && ce.Command.Category == category);

                if (entry == null)
                {
                    var command = new T();

                    entry = new GizmoCommandEntry
                    {
                        Command = command,
                        LastUpdateTime = ExternalTime
                    };
                    _updatedGizmos.Add(entry);
                }

                return entry.Command as T;
            }
        }
        #endregion
        
#endif

        #region Draw Functions
        [Conditional("UNITY_EDITOR")]
        public static void Line(string category, Vector3 start, Vector3 end, Color color, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            
            if (!Enabled) return;

            AddCommand(new LineCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                Color = color,
                RemainTime = duration,

                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void PolyLine(string category, Vector3[] points, Color color, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            
            if (!Enabled) return;

            AddCommand(new PolyLineCommand
            {
                Category = category,
                Id = id,
                Points = points,
                Color = color,
                RemainTime = duration,

                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void InterruptedLine(string category, Vector3 start, Vector3 end, Vector3 interruptPoint, Color color, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new InterruptedLineCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                InterruptPos = interruptPoint,
                Color = color,
                RemainTime = duration,

                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void InterruptedLine(string category, Vector3 start, Vector3 end, float interruptDistance, Color color, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            Vector3 interruptPoint = end;
            var dir = end - start;
            if (dir.magnitude > interruptDistance)
            {
                interruptPoint = (start + dir.normalized * interruptDistance);
            }

            AddCommand(new InterruptedLineCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                InterruptPos = interruptPoint,
                Color = color,
                RemainTime = duration,

                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Arrow(string category, Vector3 start, Vector3 end, Color color, float headSize, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new ArrowCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                Color = color,
                RemainTime = duration,

                HeadSize = headSize,
                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void InterruptedArrow(string category, Vector3 start, Vector3 end, Vector3 interruptPoint, Color color, float headSize, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new InterruptedArrowCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                InterruptPos = interruptPoint,
                Color = color,
                RemainTime = duration,
                
                HeadSize = headSize,
                DotSize = dotSize
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void InterruptedArrow(string category, Vector3 start, Vector3 end, float interruptDistance, Color color, float headSize, float dotSize = 0, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            Vector3 interruptPoint = end;
            var dir = end - start;
            if (dir.magnitude > interruptDistance)
            {
                interruptPoint = (start + dir.normalized * interruptDistance);
            }

            AddCommand(new InterruptedArrowCommand
            {
                Category = category,
                Id = id,
                Position = start,
                End = end,
                InterruptPos = interruptPoint,
                Color = color,
                RemainTime = duration,
                
                HeadSize = headSize,
                DotSize = dotSize
            });
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void Text(string category, string text, Vector3 position, Color color, int fontSize, float duration = 0, float riseSpeed = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new TextCommand
            {
                Category = category,
                Id = id,
                Position = position,
                Color = color,
                RemainTime = duration,

                Text = text,
                FontSize = fontSize,
                RiseSpeed = riseSpeed
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Disc(string category, Vector3 position, Vector3 normal, float radius, Color color, float duration = 0, long id = 0, bool solid = false)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new DiscCommand
            {
                Solid = solid,
                Category = category,
                Id = id,
                Position = position,
                Color = color,
                RemainTime = duration,

                Normal = normal,
                Radius = radius,
            });
#endif
        }
        
        
        [Conditional("UNITY_EDITOR")]
        public static void Sphere(string category, Vector3 position, float radius, Color color, float duration = 0, long id = 0, bool solid = false)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new SphereCommand
            {
                Solid = solid,
                Category = category,
                Id = id,
                Position = position,
                Color = color,
                RemainTime = duration,

                Radius = radius,
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Box(string category, Vector3 position, Vector3 size, Color color, Quaternion rotation, float duration = 0, long id = 0, bool solid = false)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new BoxCommand
            {
                Solid = solid,
                Category = category,
                Id = id,
                Position = position,
                Size = size,
                Color = color,
                RemainTime = duration,

                Rotation = rotation,
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Cone(string category, Vector3 start, Vector3 end, float angle, Color color, float period = 0f, bool drawLines = true, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new ConeCommand
            {
                Category = category,
                Id = id,
                Position = start,
                Color = color,
                RemainTime = duration,

                End = end,
                Angle = angle,
                Period =  period,
                DrawLines = drawLines
            });
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Sector(string category, Vector3 start, Vector3 end, float angle, Color color, float duration = 0, long id = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            AddCommand(new SectorCommand
            {
                Category = category,
                Id = id,
                Position = start,
                Color = color,
                RemainTime = duration,

                End = end,
                Angle = angle
            });
#endif
        }
        #endregion

        #region Update Draw Functions
        [Conditional("UNITY_EDITOR")]
        public static void UpdateLine(string category, long id, Vector3 start, Vector3 end, Color color, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            var command = GetCommand<LineCommand>(category, id);
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.Color = color;

            command.End = end;
            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdatePolyLine(string category, long id, Vector3[] points, Color color, float dotSize = 0)
        {
#if UNITY_EDITOR
            
            if (!Enabled) return;

            var command = GetCommand<PolyLineCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Points = points;
            command.Color = color;

            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateInterruptedLine(string category, long id, Vector3 start, Vector3 end, Vector3 interruptPoint, Color color, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            var command = GetCommand<InterruptedLineCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.End = end;
            command.Color = color;
            command.InterruptPos = interruptPoint;

            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateInterruptedLine(string category, long id, Vector3 start, Vector3 end, float interruptDistance, Color color, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            Vector3 interruptPoint = end;
            var dir = end - start;
            if (dir.magnitude > interruptDistance)
            {
                interruptPoint = (start + dir.normalized * interruptDistance);
            }
            
            var command = GetCommand<InterruptedLineCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.End = end;
            command.Color = color;
            command.InterruptPos = interruptPoint;

            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateArrow(string category, long id, Vector3 start, Vector3 end, Color color, float headSize, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<ArrowCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.End = end;
            command.Color = color;

            command.HeadSize = headSize;
            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateInterruptedArrow(string category, long id, Vector3 start, Vector3 end, Vector3 interruptPoint, Color color, float headSize, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            var command = GetCommand<InterruptedArrowCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.End = end;
            command.InterruptPos = interruptPoint;
            command.Color = color;

            command.HeadSize = headSize;
            command.DotSize = dotSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateInterruptedArrow(string category, long id, Vector3 start, Vector3 end, float interruptDistance, Color color, float headSize, float dotSize = 0)
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            Vector3 interruptPoint = end;
            var dir = end - start;
            if (dir.magnitude > interruptDistance)
            {
                interruptPoint = (start + dir.normalized * interruptDistance);
            }
            
            var command = GetCommand<InterruptedArrowCommand>(category, id);
            
            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.End = end;
            command.InterruptPos = interruptPoint;
            command.Color = color;

            command.HeadSize = headSize;
            command.DotSize = dotSize;
#endif

        }

        [Conditional("UNITY_EDITOR")]
        public static void UpdateText(string category, long id, string text, Vector3 position, Color color, int fontSize )
        {
#if UNITY_EDITOR
            if (!Enabled) return;

            var command = GetCommand<TextCommand>(category, id);

            command.Category = category;
            command.Id = id;
            command.Position = position;
            command.Color = color;

            command.Text = text;
            command.FontSize = fontSize;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateDisc(string category, long id, Vector3 position, Vector3 normal, float radius, Color color)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<DiscCommand>(category, id);

            command.Solid = false;
            command.Category = category;
            command.Id = id;
            command.Position = position;
            command.Color = color;

            command.Normal = normal;
            command.Radius = radius;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateSolidDisc(string category, long id, Vector3 position, Vector3 normal, float radius, Color color)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<DiscCommand>(category, id);

            command.Solid = true;
            command.Category = category;
            command.Id = id;
            command.Position = position;
            command.Color = color;

            command.Normal = normal;
            command.Radius = radius;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateSphere(string category, long id, Vector3 position, float radius, Color color, bool solid = false)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<SphereCommand>(category, id);

            command.Solid = solid;
            command.Category = category;
            command.Id = id;
            command.Position = position;
            command.Color = color;
            command.Radius = radius;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateBox(string category, long id, Vector3 position, Vector3 size, Color color, Quaternion rotation, bool solid = false)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<BoxCommand>(category, id);

            command.Solid = solid;
            command.Category = category;
            command.Id = id;
            command.Position = position;
            command.Color = color;
            command.Size = size;
            command.Rotation = rotation;
#endif
        }
        
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateCone(string category, long id, Vector3 start, Vector3 end, float angle, Color color, float period = 0f, bool drawLines = true)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<ConeCommand>(category, id);

            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.Color = color;
            command.End = end;
            command.Angle = angle;
            command.Period = period;
            command.DrawLines = drawLines;
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void UpdateSector(string category, long id, Vector3 start, Vector3 end, float angle, Color color)
        {
#if UNITY_EDITOR
            if (!Enabled) return;
            
            var command = GetCommand<SectorCommand>(category, id);

            command.Category = category;
            command.Id = id;
            command.Position = start;
            command.Color = color;
            command.End = end;
            command.Angle = angle;
#endif
        }
        #endregion
    }
}