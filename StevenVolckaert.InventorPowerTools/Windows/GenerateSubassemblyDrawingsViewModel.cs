﻿namespace StevenVolckaert.InventorPowerTools.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Inventor;

    internal class GenerateSubassemblyDrawingsViewModel : GenerateDrawingsViewModelBase
    {
        private List<Assembly> _subassemblies;
        public List<Assembly> Subassemblies
        {
            get { return _subassemblies; }
            set
            {
                if (_subassemblies == value)
                    return;

                _subassemblies = value;
                RaisePropertyChanged(() => Subassemblies);

                _documents = value.Cast<IDocument>().ToList();
                ComputeIsEverythingSelected();
            }
        }

        private Assembly _selectedSubassembly;
        public Assembly SelectedSubassembly
        {
            get { return _selectedSubassembly; }
            set
            {
                if (_selectedSubassembly == value)
                    return;

                _selectedSubassembly = value;
                RaisePropertyChanged(() => SelectedSubassembly);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenerateSubassemblyDrawingsViewModel"/> class.
        /// </summary>
        public GenerateSubassemblyDrawingsViewModel()
        {
            Title = "Generate Subassembly Drawings";
            SelectedViewStyle = DrawingViewStyleEnum.kHiddenLineDrawingViewStyle;
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
                    // 1. Alter formatting of custom properties.
                    SetCustomPropertyFormat(assembly.Parts);

                    // 2. Add base view.
                    var baseView = sheet.DrawingViews.AddBaseView(
                        Model: (_Document)assembly.Document,
                        Position: drawingDocument.ActiveSheet.CenterPoint(),
                        Scale: Scale,
                        ViewOrientation: ViewOrientationTypeEnum.kFrontViewOrientation,
                        ViewStyle: SelectedViewStyle,
                        ModelViewName: string.Empty,
                        ArbitraryCamera: Type.Missing,
                        AdditionalOptions: Type.Missing
                    );

                    baseView.AddTopAndLeftProjectedViews(
                        addDimensions: true,
                        drawingDistance: 0.5
                    );

                    baseView.AddPartName(
                        partName: baseView.ReferencedDocumentDescriptor.DisplayName.RemoveExtension(),
                        drawingDistance: 0.5
                    );

                    // 3. Add part list to the top right corner.
                    var partsList = sheet.AddPartsList(assembly.Document, PartsListLevelEnum.kStructured);

                    // 4. Add base "ISO TOP Right", Hidden line removed, Shaded base view of the subassembly in the drawing's top right corner.
                    var perspectiveView = sheet.DrawingViews.AddBaseView(
                        Model: (_Document)assembly.Document,
                        Position: drawingDocument.ActiveSheet.TopRightPoint(),
                        Scale: PerspectiveScale,
                        ViewOrientation: ViewOrientationTypeEnum.kIsoTopRightViewOrientation,
                        ViewStyle: SelectedViewStyle,
                        ModelViewName: string.Empty,
                        ArbitraryCamera: Type.Missing,
                        AdditionalOptions: Type.Missing
                    );

                    var margin = sheet.Margin();

                    perspectiveView.Fit(
                        new Rectangle(
                            AddIn.CreatePoint2D(
                                x: ((sheet.Width - margin.Right) * 3 + margin.Left) / 4 + 1,
                                y: ((sheet.Height - margin.Top) * 3 + margin.Bottom) / 4 + 1
                            ),
                            AddIn.CreatePoint2D(
                                x: topRightCorner.X - 1,
                                y: topRightCorner.Y - 1
                            )
                        )
                    );

                    perspectiveView.Position =
                        AddIn.CreatePoint2D(
                            x: perspectiveView.Position.X,
                            y: perspectiveView.Position.Y - partsList.RangeBox.Height()
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
