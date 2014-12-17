﻿using System;
using System.Collections.Generic;
using System.Linq;
using Inventor;

namespace StevenVolckaert.InventorPowerTools.Windows
{
    internal class GenerateSubassemblyDrawingsViewModel : GenerateDrawingsViewModelBase
    {
        private List<Assembly> _subassemblies;
        public List<Assembly> Subassemblies
        {
            get { return _subassemblies; }
            set
            {
                if (_subassemblies != value)
                {
                    _subassemblies = value;
                    RaisePropertyChanged(() => Subassemblies);

                    _documents = value.Cast<IDocument>().ToList();
                    CalculateIsEverythingSelected();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateSubassemblyDrawingsViewModel"/> class.
        /// </summary>
        public GenerateSubassemblyDrawingsViewModel()
        {
            Title = "Generate Subassembly Drawings";
        }

        protected override void GenerateDrawings()
        {
            if (Subassemblies == null)
                return;

            var selectedSubassemblies = Subassemblies.Where(x => x.IsSelected == true).ToList();

            if (selectedSubassemblies.Count == 0)
            {
                ShowWarningMessageBox("No subassemblies are selected.");
                return;
            }

            foreach (var assembly in selectedSubassemblies)
            {
                var drawingDocument = CreateDrawingDocument();
                var sheet = drawingDocument.ActiveSheet;
                var topRightCorner = sheet.TopRightCorner();

                try
                {
                    // 1. Add base view.
                    var baseView = sheet.DrawingViews.AddBaseView(
                        Model: (_Document)assembly.Document,
                        Position: drawingDocument.ActiveSheet.CenterPoint(),
                        Scale: Scale,
                        ViewOrientation: ViewOrientationTypeEnum.kFrontViewOrientation,
                        ViewStyle: DrawingViewStyleEnum.kHiddenLineDrawingViewStyle,
                        ModelViewName: string.Empty,
                        ArbitraryCamera: Type.Missing,
                        AdditionalOptions: Type.Missing
                    );

                    baseView.AddTopAndLeftProjectedViews(
                        addDimensions: true,
                        drawingDistance: 0.5
                    );

                    // 2. Add part list to the top right corner.
                    foreach (var part in assembly.Document.Parts())
                    {
                        // Alter formatting of custom properties.
                        part.SetCustomPropertyFormat("Lengte", displayPrecision: CustomPropertyPrecisionEnum.kZeroDecimalPlacePrecision, showUnit: false);
                        part.SetCustomPropertyFormat("Breedte", displayPrecision: CustomPropertyPrecisionEnum.kZeroDecimalPlacePrecision, showUnit: false);
                        part.SetCustomPropertyFormat("Dikte", displayPrecision: CustomPropertyPrecisionEnum.kZeroDecimalPlacePrecision, showUnit: false);
                    }

                    var partsList = sheet.AddPartsList(assembly.Document, PartsListLevelEnum.kStructured);

                    // 3. Add base "ISO TOP Right", Hidden line removed, Shaded base view of the subassembly in the drawing's top right corner.
                    var perspectiveView = sheet.DrawingViews.AddBaseView(
                        Model: (_Document)assembly.Document,
                        Position: drawingDocument.ActiveSheet.TopRightPoint(),
                        Scale: PerspectiveScale,
                        ViewOrientation: ViewOrientationTypeEnum.kIsoTopRightViewOrientation,
                        ViewStyle: DrawingViewStyleEnum.kShadedDrawingViewStyle,
                        ModelViewName: string.Empty,
                        ArbitraryCamera: Type.Missing,
                        AdditionalOptions: Type.Missing
                    );

                    var margin = sheet.Margin();

                    perspectiveView.Fit(
                        new Rectangle(
                            AddIn.CreatePoint2D(
                                ((sheet.Width - margin.Right) * 3 + margin.Left) / 4 + 1,
                                ((sheet.Height - margin.Top) * 3 + margin.Bottom) / 4 + 1
                            ),
                            AddIn.CreatePoint2D(
                                topRightCorner.X - 1,
                                topRightCorner.Y - 1
                            )
                        )
                    );

                    perspectiveView.Position =
                        AddIn.CreatePoint2D(
                            perspectiveView.Position.X,
                            perspectiveView.Position.Y - partsList.RangeBox.Height()
                        );
                }
                catch (Exception ex)
                {
                    ShowWarningMessageBox(ex.ToString());
                }
            }
        }
    }
}