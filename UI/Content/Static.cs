using System;
using System.Collections.Generic;
using System.Linq;

namespace DUIP.UI
{
    /// <summary>
    /// A content representation static instance of a certain type, possibly mirrored on a network.
    /// </summary>
    public class StaticContent<T> : Content
    {
        public StaticContent(T Value, Type<T> Type)
        {
            this._Value = Value;
            this._Type = Type;
        }

        public override Disposable<Visual> CreateVisual()
        {
            // Use a generic control for now
            if (_Typeface.Object == null)
            {
                _Typeface = BitmapTypeface.Create(BitmapTypeface.GetFamily("Arial"), Font.ASCIICharacters, System.Drawing.FontStyle.Regular, 3, 60.0f, 512);
            }
            BitmapFont font = _Typeface.Object.GetFont(0.05, Color.Black);

            FlowBlock testflow = new FlowBlock
            {
                Style = new FlowStyle
                {
                    Direction = FlowDirection.RightDown,
                    Justification = FlowJustification.Justify,
                    LineAlignment = Alignment.Center,
                    LineSpacing = 0.00,
                    LineSize = 0.01
                }
            };
            testflow.AddText(
                "Lorem ipsum dolor sit amet, consec tetur adipis cing elit. Nunc sus cipit phare tra nunc, " +
                "sit amet fauc ibus risus sceler isque ac. Etiam condi mentum justo quis dolor vehi cula ac volut pat " +
                "tortor adi piscing. Donec tinci dunt quam quis orci pel lent esque feug iat. Fusce eget nisi ac mi " +
                "trist ique port titor. Aliq uam et males uada elit. Suspen disse elei fend hend rerit semper. ", font);

            Block testblock = testflow
                .WithPad(0.05)
                .WithSize(1.0, 1.0)
                .WithBorder(new Border
                {
                    Color = Color.RGB(0.8, 0.2, 0.2),
                    Weight = 0.04,
                })
                .WithBackground(Color.RGB(0.9, 0.5, 0.5));

            Rectangle sizerange = new Rectangle(1.0, 1.0, 3.0, 3.0);

            return (Control)testblock.CreateControl(sizerange, null);
        }
        private static Disposable<BitmapTypeface> _Typeface = null;

        /// <summary>
        /// Gets the value for this content.
        /// </summary>
        public T Value
        {
            get
            {
                return this._Value;
            }
        }

        /// <summary>
        /// Gets the type for this content.
        /// </summary>
        public Type<T> Type
        {
            get
            {
                return this._Type;
            }
        }

        private T _Value;
        private Type<T> _Type;
    }
}