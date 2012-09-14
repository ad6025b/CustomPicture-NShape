using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;

namespace Dataweb.NShape.Advanced
{
    /// <summary>
    /// Displays a bitmap in the diagram.
    /// </summary>
    public class CustomPictureBase : RectangleBase {


        protected string resourceName;
        protected Assembly resourceAssembly;

        #region [Public] Properties

        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("The shape's image.")]
        [RequiredPermission(Permission.Present)]
        [Editor("Dataweb.NShape.WinFormsUI.NamedImageUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
        public NamedImage Image {
            get { return image; }
            set {
                GdiHelpers.DisposeObject(ref brushImage);
                if (NamedImage.IsNullOrEmpty(value))
                    image = null;
                else image = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }


        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("Defines the layout of the displayed image.")]
        [PropertyMappingId(PropertyIdImageLayout)]
        [RequiredPermission(Permission.Present)]
        public ImageLayoutMode ImageLayout {
            get { return imageLayout; }
            set {
                imageLayout = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }


        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("Displays the image as grayscale image.")]
        [PropertyMappingId(PropertyIdImageGrayScale)]
        [RequiredPermission(Permission.Present)]
        public bool GrayScale {
            get { return imageGrayScale; }
            set {
                imageGrayScale = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }


        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("Gamma correction value for the image.")]
        [PropertyMappingId(PropertyIdImageGamma)]
        [RequiredPermission(Permission.Present)]
        public float GammaCorrection {
            get { return imageGamma; }
            set {
                if (value <= 0) throw new ArgumentOutOfRangeException("Value has to be greater 0.");
                imageGamma = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }


        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("Transparency of the image in percentage.")]
        [PropertyMappingId(PropertyIdImageTransparency)]
        [RequiredPermission(Permission.Present)]
        public byte Transparency {
            get { return imageTransparency; }
            set {
                if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("Value has to be between 0 and 100.");
                imageTransparency = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }


        /// <ToBeCompleted></ToBeCompleted>
        [Category("Appearance")]
        [Description("Transparency of the image in percentage.")]
        [PropertyMappingId(PropertyIdImageTransparentColor)]
        [RequiredPermission(Permission.Present)]
        public Color TransparentColor {
            get { return transparentColor; }
            set {
                transparentColor = value;
                InvalidateDrawCache();
                Invalidate();
            }
        }

        #endregion


        #region [Public] Methods

        /// <ToBeCompleted></ToBeCompleted>
        public void FitShapeToImageSize() {
            if (image != null) {
                Width = image.Width + (Width - imageBounds.Width);
                Height = image.Height + (Height - imageBounds.Height);
            }
        }


        /// <override></override>
        public override Shape Clone() {
            Shape result = new CustomPictureBase(Type, (Template)null);
            result.CopyFrom(this);
            return result;
        }


        /// <override></override>
        public override void CopyFrom(Shape source) {
            base.CopyFrom(source);
            if (source is CustomPictureBase) {
                CustomPictureBase src = (CustomPictureBase)source;
                if (!NamedImage.IsNullOrEmpty(src.Image))
                    Image = src.Image.Clone();
                imageGrayScale = src.GrayScale;
                imageLayout = src.ImageLayout;
                imageGamma = src.GammaCorrection;
                imageTransparency = src.Transparency;
                transparentColor = src.TransparentColor;
                compressionQuality = src.compressionQuality;
            }
        }


        /// <override></override>
        public override void MakePreview(IStyleSet styleSet) {
            base.MakePreview(styleSet);
            isPreview = true;
            GdiHelpers.DisposeObject(ref imageAttribs);
            GdiHelpers.DisposeObject(ref imageBrush);
            //if (!NamedImage.IsNullOrEmpty(image) && BrushImage != null)
            //   image.Image = BrushImage;
        }


        /// <override></override>
        public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
            return base.HitTest(x, y, controlPointCapability, range);
        }


        /// <override></override>
        public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
            //if (ImageLayout == ImageLayoutMode.Original) {
            //    if ((controlPointCapability & ControlPointCapabilities.Glue) != 0)
            //        return false;
            //    if ((controlPointCapability & ControlPointCapabilities.Connect) != 0 ) {
            //        return (controlPointId != RotateControlPointId && IsConnectionPointEnabled(controlPointId));
            //    }
            //    if ((controlPointCapability & ControlPointCapabilities.Reference) != 0) {
            //        if (controlPointId == RotateControlPointId || controlPointId == ControlPointId.Reference)
            //            return true;
            //    }
            //    if ((controlPointCapability & ControlPointCapabilities.Rotate) != 0) {
            //        if (controlPointId == RotateControlPointId)
            //            return true;
            //    }
            return base.HasControlPointCapability(controlPointId, controlPointCapability);
            //} else return base.HasControlPointCapability(controlPointId, controlPointCapability);
        }


        /// <override></override>
        public override void Draw(Graphics graphics) {
            if (graphics == null) throw new ArgumentNullException("graphics");
            UpdateDrawCache();
            Pen pen = ToolCache.GetPen(LineStyle, null, null);
            Brush brush = ToolCache.GetTransformedBrush(FillStyle, boundingRectangleUnrotated, Center, Angle);
            //Brush brush = ToolCache.GetTransformedBrush(FillStyle, GetBoundingRectangle(true), Center, Angle);
            graphics.FillPath(brush, Path);

            Debug.Assert(imageAttribs != null);
            Debug.Assert(Geometry.IsValid(imageBounds));
            if (image != null && image.Image is Metafile) {
                GdiHelpers.DrawImage(graphics, image.Image, imageAttribs, imageLayout, imageBounds, imageBounds, Geometry.TenthsOfDegreeToDegrees(Angle), Center);
            } else {
                Debug.Assert(imageBrush != null);
                graphics.FillPolygon(imageBrush, imageDrawBounds);
            }
            DrawCaption(graphics);
            graphics.DrawPath(pen, Path);
            if (Children.Count > 0) foreach (Shape s in Children) s.Draw(graphics);
        }

        #endregion


        #region IEntity Members

        /// <override></override>
        protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
            base.LoadFieldsCore(reader, version);
            imageLayout = (ImageLayoutMode)reader.ReadByte();
            imageTransparency = reader.ReadByte();
            imageGamma = reader.ReadFloat();
            compressionQuality = reader.ReadByte();
            imageGrayScale = reader.ReadBool();
            string name = reader.ReadString();
            Image img = reader.ReadImage();
            if (name != null && img != null)
                image = new NamedImage(img, name);
            transparentColor = Color.FromArgb(reader.ReadInt32());
        }


        /// <override></override>
        protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
            base.SaveFieldsCore(writer, version);
            writer.WriteByte((byte)imageLayout);
            writer.WriteByte(imageTransparency);
            writer.WriteFloat(imageGamma);
            writer.WriteByte(compressionQuality);
            writer.WriteBool(imageGrayScale);
            if (NamedImage.IsNullOrEmpty(image)) {
                writer.WriteString(string.Empty);
                writer.WriteImage(null);
            } else {
                writer.WriteString(image.Name);
                object imgTag = image.Image.Tag;
                image.Image.Tag = image.Name;
                writer.WriteImage(image.Image);
                image.Image.Tag = imgTag;
            }
            writer.WriteInt32(transparentColor.ToArgb());
        }


        /// <summary>
        /// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.CustomPictureBase" />.
        /// </summary>
        new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
            foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
                yield return pi;
            yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
            yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
            yield return new EntityFieldDefinition("ImageGammaCorrection", typeof(float));
            yield return new EntityFieldDefinition("ImageCompressionQuality", typeof(byte));
            yield return new EntityFieldDefinition("ConvertToGrayScale", typeof(bool));
            yield return new EntityFieldDefinition("ImageFileName", typeof(string));
            yield return new EntityFieldDefinition("Image", typeof(Image));
            yield return new EntityFieldDefinition("ImageTransparentColor", typeof(int));
        }

        #endregion


        #region [Protected] Overridden Methods

        /// <ToBeCompleted></ToBeCompleted>
        protected internal CustomPictureBase(ShapeType shapeType, Template template)
            : base(shapeType, template) {
            Construct();
            }

        protected internal CustomPictureBase(ShapeType shapeType, Template template, string resourceBaseName, Assembly resourceAssembly)
            : base(shapeType, template)
        {
            Construct(resourceBaseName, resourceAssembly);
        }


        /// <ToBeCompleted></ToBeCompleted>
        protected internal CustomPictureBase(ShapeType shapeType, IStyleSet styleSet)
            : base(shapeType, styleSet) {
            Construct();
            }

        protected internal CustomPictureBase(ShapeType shapeType, IStyleSet styleSet, string resourceBaseName, Assembly resourceAssembly)
            : base(shapeType, styleSet)
        {
            Construct(resourceBaseName, resourceAssembly);
        }



        /// <override></override>
        protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
            if (index != 0) throw new IndexOutOfRangeException();
            Size txtSize = Size.Empty;
            txtSize.Width = Width;
            txtSize.Height = Height;
            string txt = string.IsNullOrEmpty(Text) ? "Ip" : Text;
            if (DisplayService != null)
                txtSize = TextMeasurer.MeasureText(DisplayService.InfoGraphics, txt, CharacterStyle, txtSize, ParagraphStyle);
            else txtSize = TextMeasurer.MeasureText(txt, CharacterStyle, txtSize, ParagraphStyle);

            captionBounds = Rectangle.Empty;
            captionBounds.Width = Width;
            captionBounds.Height = Math.Min(Height, txtSize.Height);
            captionBounds.X = (int)Math.Round(-(Width / 2f));
            captionBounds.Y = (int)Math.Round((Height / 2f) - captionBounds.Height);
        }


        /// <override></override>
        protected override void InvalidateDrawCache() {
            base.InvalidateDrawCache();
            GdiHelpers.DisposeObject(ref imageAttribs);
            GdiHelpers.DisposeObject(ref imageBrush);
            imageBounds = Geometry.InvalidRectangle;
        }


        /// <override></override>
        protected override void RecalcDrawCache() {
            base.RecalcDrawCache();
            imageBounds.Width = Width;
            imageBounds.Height = Height;
            imageBounds.X = (int)Math.Round(-Width / 2f);
            imageBounds.Y = (int)Math.Round(-Height / 2f);
            if (!string.IsNullOrEmpty(Text)) {
                Rectangle r;
                CalcCaptionBounds(0, out r);
                imageBounds.Height -= r.Height;
            }
            imageDrawBounds[0].X = imageBounds.Left; imageDrawBounds[0].Y = imageBounds.Top;
            imageDrawBounds[1].X = imageBounds.Right; imageDrawBounds[1].Y = imageBounds.Top;
            imageDrawBounds[2].X = imageBounds.Right; imageDrawBounds[2].Y = imageBounds.Bottom;
            imageDrawBounds[3].X = imageBounds.Left; imageDrawBounds[3].Y = imageBounds.Bottom;

            if (imageAttribs == null)
                imageAttribs = GdiHelpers.GetImageAttributes(imageLayout, imageGamma, imageTransparency, imageGrayScale, isPreview, transparentColor);

            Image bitmapImg = null;
            if (image == null || image.Image == null)
                bitmapImg = Properties.Resources.DefaultBitmapLarge;
            else if (image.Image is Bitmap)
                bitmapImg = Image.Image;
            if (bitmapImg is Bitmap && imageBrush == null) {
                if (isPreview)
                    imageBrush = GdiHelpers.CreateTextureBrush(bitmapImg, Width, Height, imageAttribs);
                else imageBrush = GdiHelpers.CreateTextureBrush(bitmapImg, imageAttribs);
                // Transform texture Brush
                Point imageCenter = Point.Empty;
                imageCenter.Offset((int)Math.Round(imageBounds.Width / 2f), (int)Math.Round(imageBounds.Height / 2f));
                if (Angle != 0) imageCenter = Geometry.RotatePoint(Point.Empty, Geometry.TenthsOfDegreeToDegrees(Angle), imageCenter);
                GdiHelpers.TransformTextureBrush(imageBrush, imageLayout, imageBounds, imageCenter, Geometry.TenthsOfDegreeToDegrees(Angle));
            }
        }


        /// <override></override>
        protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
            base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
            if (Geometry.IsValid(imageBounds)) {
                imageBounds.Offset(deltaX, deltaY);
                Matrix.TransformPoints(imageDrawBounds);
                if (imageBrush != null) {
                    float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
                    Point p = Point.Empty;
                    p.X = (int)Math.Round(imageBounds.X + (imageBounds.Width / 2f));
                    p.Y = (int)Math.Round(imageBounds.Y + (imageBounds.Height / 2f));
                    p = Geometry.RotatePoint(Center, angleDeg, p);
                    GdiHelpers.TransformTextureBrush(imageBrush, imageLayout, imageBounds, p, angleDeg);
                }
            }
        }


        /// <override></override>
        protected override bool CalculatePath() {
            if (base.CalculatePath()) {
                int left = (int)Math.Round(-Width / 2f);
                int top = (int)Math.Round(-Height / 2f);

                Rectangle shapeRect = Rectangle.Empty;
                shapeRect.Offset(left, top);
                shapeRect.Width = Width;
                shapeRect.Height = Height;

                Path.Reset();
                Path.StartFigure();
                Path.AddRectangle(shapeRect);
                Path.CloseFigure();
                return true;
            } else return false;
        }

        #endregion

		
        /// <override></override>
        protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
            switch (propertyMapping.ShapePropertyId) {
                    //case PropertyIdImage: break;
                case PropertyIdImageLayout:
                    ImageLayout = (ImageLayoutMode)propertyMapping.GetInteger();
                    break;
                case PropertyIdImageGrayScale:
                    GrayScale = (propertyMapping.GetInteger() != 0);
                    break;
                case PropertyIdImageGamma:
                    //GammaCorrection = Math.Max(0.00000001f, Math.Abs(propertyMapping.GetFloat()));
                    GammaCorrection = propertyMapping.GetFloat();
                    break;
                case PropertyIdImageTransparency:
                    checked {
                        Transparency = (byte)propertyMapping.GetInteger();
                    }
                    break;
                case PropertyIdImageTransparentColor:
                    TransparentColor = Color.FromArgb(propertyMapping.GetInteger());
                    break;
                default:
                    base.ProcessExecModelPropertyChange(propertyMapping);
                    break;
            }
        }


        //private Image BrushImage {
        //   get {
        //      if (brushImage == null
        //         && !NamedImage.IsNullOrEmpty(image)
        //         && (image.Width >= 2 * Width || image.Height >= 2 * Height))
        //            brushImage = GdiHelpers.GetBrushImage(image.Image, Width, Height);
        //      return brushImage;
        //   }
        //}


        private void Construct(string resourceBaseName, Assembly resourceAssembly)
        {
            if (resourceBaseName == null) throw new ArgumentNullException("resourceBaseName");
            System.IO.Stream stream = resourceAssembly.GetManifestResourceStream(resourceBaseName);
            if (stream == null) throw new ArgumentException(string.Format("'{0}' is not a valid resource in '{1}'.", resourceBaseName, resourceAssembly), "resourceBaseName");
            
            var customimage = System.Drawing.Image.FromStream(stream);
            if (customimage == null) throw new ArgumentException(string.Format("'{0}' is not a valid image resource.", resourceBaseName), "resourceBaseName");
            this.resourceName = resourceBaseName;
            this.resourceAssembly = resourceAssembly;

            image = new NamedImage(customimage, "customimage");

            // this fillStyle holds the image of the shape
            imageGrayScale = false;
            compressionQuality = 100;
            imageGamma = 1;
            imageLayout = ImageLayoutMode.Original;
            imageTransparency = 0;


            //FitShapeToImageSize();
        }


        private void Construct()
        {
            // this fillStyle holds the image of the shape
            image = null;
            imageGrayScale = false;
            compressionQuality = 100;
            imageGamma = 1;
            imageLayout = ImageLayoutMode.Original;
            imageTransparency = 0;
        }


        #region Fields

        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImage = 9;
        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImageLayout = 10;
        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImageGrayScale = 11;
        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImageGamma = 12;
        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImageTransparency = 13;
        /// <ToBeCompleted></ToBeCompleted>
        protected const int PropertyIdImageTransparentColor = 14;

        private const int RotateControlPointId = 9;

        private ImageAttributes imageAttribs = null;
        private TextureBrush imageBrush = null;
        private Rectangle imageBounds = Geometry.InvalidRectangle;
        private Point[] imageDrawBounds = new Point[4];
        private bool isPreview = false;
        private Size fullImageSize = Size.Empty;
        private Size currentImageSize = Size.Empty;
        private Image brushImage;

        private NamedImage image;
        private ImageLayoutMode imageLayout = ImageLayoutMode.Fit;
        private byte imageTransparency = 0;
        private float imageGamma = 1.0f;
        private bool imageGrayScale = false;
        private byte compressionQuality = 100;
        private Color transparentColor = Color.Empty;
		
        #endregion
    }
}