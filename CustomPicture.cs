using System.Reflection;
using Dataweb.NShape.Advanced;

namespace Dataweb.NShape.GeneralShapes
{
    public class CustomPicture : CustomPictureBase {

        internal static CustomPicture CreateInstance(ShapeType shapeType, Template template, string resourceBasename, Assembly resourceAssembly)
        {
            return new CustomPicture(shapeType, template, resourceBasename,  resourceAssembly);
        }


        /// <override></override>
        public override Shape Clone() {
            Shape result = new CustomPicture(Type, (Template)null, resourceName, resourceAssembly);
            result.CopyFrom(this);
            return result;
        }


        protected internal CustomPicture(ShapeType shapeType, Template template, string resourceBasename, Assembly resourceAssembly)
            : base(shapeType, template, resourceBasename, resourceAssembly) {
            Construct();
            }


        protected internal CustomPicture(ShapeType shapeType, IStyleSet styleSet)
            : base(shapeType, styleSet) {
            Construct();
            }


        private void Construct() {
            //Image = new NamedImage();
        }

    }
}