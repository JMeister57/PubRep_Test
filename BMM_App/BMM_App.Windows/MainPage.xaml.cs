using BMM_App.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Data.Json;
using Windows.Storage.Streams;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace BMM_App
{

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {      
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private string username = "Sebastian";

        private string filename = "unsaved diagram";
        public static List<BaseModel> models = new List<BaseModel>();
        private static List<Link> links = new List<Link>();

        private bool linking = false;
        private Link currentLine;
        private BaseModel sourceModel;
        
        private bool selecting;
        private Point selectionStartPoint;
        private Rectangle selectionBox;

        private bool dragging; 

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            DataContext = this;
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; OnPropertyChanged("Username"); }
        }

        public string Header
        {
            get { return "Business Motivation Model: " + filename; }
            set { filename = value; OnPropertyChanged("Header"); }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // restore session data
            if (e.PageState != null && e.PageState.ContainsKey("key"))
            {

            }
            // restore app data
            //ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;
            //if (roaming.Containers.ContainsKey("user"))
            //{
            //    var username = (roaming.Containers["user"].Values.ContainsKey("name")) ? (string)roaming.Containers["user"].Values["name"] : "";
            //    var passwd = (roaming.Containers["user"].Values.ContainsKey("password")) ? (string)roaming.Containers["user"].Values["name"] : "";
            //}
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            //ApplicationDataContainer roaming = ApplicationData.Current.RoamingSettings;
            //var container = roaming.CreateContainer("user", ApplicationDataCreateDisposition.Always);
            //if (roaming.Containers.ContainsKey("user"))
            //{
            //    roaming.Containers["user"].Values["name"] = user;
            //    var buffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            //    var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            //    var hash = hasher.HashData(buffer);
            //    roaming.Containers["user"].Values["password"] = CryptographicBuffer.EncodeToBase64String(hash);
            //}
        }

        #region NavigationHelper-Registrierung
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        private void StackPanel_Drop(object sender, DragEventArgs e)
        {
            object item;
            if (e.Data.Properties.TryGetValue("Item", out item))
            {
                Category category = (Category)item;
                BaseModel model =
                            (category == Category.NOTE) ? (BaseModel)new NoteModel(username) :
                            (category == Category.ASSESSMENT) ? (BaseModel)new AssessmentModel(username) :
                            (category == Category.BUSINESS_RULE) ? (BaseModel)new BusinessRuleModel(username) :
                            (category == Category.INFLUENCER) ? (BaseModel)new InfluencerModel(username) :
                            (BaseModel)new BMM(category, username);

                model.LinkStartEvent += OnLinkStart;
                model.LinkEndEvent += OnLinkEnd;
                model.DeleteEvent += DeleteModel;
                model.MovedEvent += ModelMoved;
                model.MoveEndEvent += ModelStoppedMoving;
                models.Add(model);
                if (model is BMM)
                {
                    BMM bpmm = (BMM)model;
                    bpmm.BMMTappedEvent += infobar.BMMTapped;
                    bpmm.WarningsAddedEvent += infobar.AddWarnings;
                    bpmm.WarningsRemovedEvent += infobar.RemoveWarnings;

                    foreach (var warnItem in bpmm.getWarnings())
                    {
                        infobar.Warnings.Add(warnItem);
                    }
                }
                Point pos = e.GetPosition(workspace);
                Canvas.SetLeft(model, pos.X);
                Canvas.SetTop(model, pos.Y);
                workspace.Children.Add(model);
                if (step == TourStep.V1 && category == Category.VISION
                    || step == TourStep.G1 && category == Category.GOAL
                    || step == TourStep.O1 && category == Category.OBJECTIVE
                    || step == TourStep.S1 && category == Category.STRATEGY
                    || step == TourStep.T1 && category == Category.TACTIC
                    || step == TourStep.I1 && category == Category.INFLUENCER
                    || step == TourStep.A1 && category == Category.ASSESSMENT
                    || step == TourStep.P1 && category == Category.BUSINESS_POLICY
                    || step == TourStep.R1 && category == Category.BUSINESS_RULE)
                {
                    performStep(step + 1);
                }
            }
        }

        public static BaseModel getModel(int id)
        {
            foreach (var model in models)
            {
                if (model.id == id)
                {
                    return model;
                }
            }
            return null;
        }

        private double maxX()
        {
            double maxX = 0;
            foreach (var model in models)
            {
                model.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                maxX = Math.Max(maxX, Canvas.GetLeft(model) + model.ActualWidth);
            }
            return maxX;
        }

        private double minX()
        {
            double minX = double.PositiveInfinity;
            foreach (var model in models)
            {
                minX = Math.Min(minX, Canvas.GetLeft(model));
            }
            return minX;
        }

        private double maxY()
        {
            double maxY = 0;
            foreach (var model in models)
            {
                model.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                maxY = Math.Max(maxY, Canvas.GetTop(model) + model.ActualHeight);
            }
            return maxY;
        }

        private double minY()
        {
            double minY = double.PositiveInfinity;
            foreach (var model in models)
            {
                minY = Math.Min(minY, Canvas.GetTop(model));
            }
            return minY;
        }

        private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items[0].Equals(visionIcon)) { e.Data.Properties.Add("Item", Category.VISION); }
            else if (e.Items[0].Equals(goalIcon)) { e.Data.Properties.Add("Item", Category.GOAL); }
            else if (e.Items[0].Equals(objectiveIcon)) { e.Data.Properties.Add("Item", Category.OBJECTIVE); }
            else if (e.Items[0].Equals(missionIcon)) { e.Data.Properties.Add("Item", Category.MISSION); }
            else if (e.Items[0].Equals(strategyIcon)) { e.Data.Properties.Add("Item", Category.STRATEGY); }
            else if (e.Items[0].Equals(tacticIcon)) { e.Data.Properties.Add("Item", Category.TACTIC); }
            else if (e.Items[0].Equals(policyIcon)) { e.Data.Properties.Add("Item", Category.BUSINESS_POLICY); }
            else if (e.Items[0].Equals(ruleIcon)) { e.Data.Properties.Add("Item", Category.BUSINESS_RULE); }
            else if (e.Items[0].Equals(influencerIcon)) { e.Data.Properties.Add("Item", Category.INFLUENCER); }
            else if (e.Items[0].Equals(assessmentIcon)) { e.Data.Properties.Add("Item", Category.ASSESSMENT); }
            else if (e.Items[0].Equals(note)) { e.Data.Properties.Add("Item", Category.NOTE); }
        }
        #region link drawing
        public void OnLinkStart(object sender, PointerRoutedEventArgs e)
        {
            sourceModel = (BaseModel)sender;
            Point p = e.GetCurrentPoint(workspace).Position;
            currentLine = new Link(sourceModel, p, p);
            sourceModel.MovedEvent += currentLine.sourceMoved;
            currentLine.DeleteEvent += DeleteLink;
            links.Add(currentLine);
            workspace.Children.Add(currentLine);
            linking = true;
        }

        public void OnLinkEnd(object sender, EventArgs e)
        {
            if (currentLine == null)
            { // case: simple click on model
                return;
            }

            BaseModel target = (BaseModel)sender;

            if (target == sourceModel)
            { // case: link to itself
                Debug.WriteLine("Cannot pull link to itself.");
                workspace.Children.Remove(currentLine);
                links.Remove(currentLine);
                return;
            }
            linkModels(sourceModel, target);
            target.MovedEvent += currentLine.targetMoved;
            sourceModel.anchor.Opacity = 0.4;
            if (sourceModel is BMM && target is BMM)
            {
                ((BMM)sourceModel).validateNewLink(((BMM)target).category);
                ((BMM)target).validateNewLink(((BMM)sourceModel).category);
            }
            sourceModel = null;
            currentLine = (step == TourStep.None)? null : currentLine;
            linking = false;
        }

        private void linkModels(BaseModel source, BaseModel target)
        {
            var s = new Point(Canvas.GetLeft(source), Canvas.GetTop(source));
            var s2 = new Point(s.X + source.GetWidth(), s.Y + source.GetHeight()) ;
            var t = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));
            var t2 = new Point(t.X + target.GetWidth(), t.Y + target.GetHeight());

            if (s2.X < t.X)
            { // case: target is right from source
                currentLine.updateStartPoint(source, new Point(s.X + source.GetWidth(), s.Y + source.GetHeight() / 2));
                var targetPoint =
                    (s.Y > t2.Y) ? new Point(t.X + target.GetWidth() / 2, t2.Y) : // target right and above of source
                    (s2.Y < t.Y) ? new Point(t.X + target.GetWidth() / 2, t.Y) : // target right and below source
                    new Point(t.X, t.Y + target.GetHeight() / 2); // target right of source
                currentLine.updateEndPoint(target, targetPoint);
                    
            }
            else if (s.X > t2.X)
            { // case: target is left from source
                currentLine.updateStartPoint(source, new Point(s.X, s.Y + source.GetHeight() / 2));
                var targetPoint =
                    (s.Y > t2.Y) ? new Point(t.X + target.GetWidth() / 2, t2.Y) : // target left and above of source
                    (s2.Y < t.Y) ? new Point(t.X + target.GetWidth() / 2, t.Y) : // target left and below of source
                    new Point(t2.X, t.Y + target.GetHeight() / 2); // target left of source
                currentLine.updateEndPoint(target, targetPoint);
            }
            else if (s.Y > t2.Y)
            { // case: target is neither left nor right of source, but above of it
                currentLine.updateStartPoint(source, new Point(s.X + source.GetWidth() / 2, s.Y));
                currentLine.updateEndPoint(target, new Point(t.X + target.GetWidth() / 2, t2.Y));
            }
            else
            { // case: target is neither left nor right of source, but below it 
                currentLine.updateStartPoint(source, new Point(s.X + source.GetWidth() / 2, s2.Y));
                currentLine.updateEndPoint(target, new Point(t.X + target.GetWidth() / 2, t.Y));
            }
        }
        #endregion

        private void workspace_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (linking)
            {
                if (e.IsInertial)
                {
                    e.Handled = true;
                    return;
                }
                Point target = e.Position;
                target.X -= 5;
                target.Y -= 5;
                currentLine.updateEndPoint(null, target);
            }
            else if (e.Delta.Scale != 0)
            {
                foreach (var model in models)
                {

                    model.Resize(e.Delta.Scale);
                    model.UpdateFontSize(e.Delta.Scale);
                }
                foreach (var link in links)
                {
                    link.UpdateFontSize(e.Delta.Scale);
                }
                workspace.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                workspace.Width = Math.Max(maxX(), workspace.ActualWidth);
                workspace.Height = Math.Max(maxY(), workspace.ActualHeight);
            }
            else if(dragging)
            {
                workspaceScroll.ChangeView(workspaceScroll.HorizontalOffset + e.Delta.Translation.X,
                    workspaceScroll.VerticalOffset + e.Delta.Translation.Y, 1);
            }
        }

        private void workspace_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (linking)
            {
                linking = false;
                workspace.Children.Remove(currentLine);
                links.Remove(currentLine);
                currentLine = null;
                return;
            }
            dragging = true;
            workspace.CapturePointer(e.Pointer);
            //selecting = true;
            //selectionStartPoint = e.GetCurrentPoint(workspace).Position;
            //selectionBox = new Rectangle()
            //{
            //    Width = Math.Abs(selectionStartPoint.X - selectionStartPoint.X),
            //    Height = Math.Abs(selectionStartPoint.Y - selectionStartPoint.Y),
            //    Fill = new SolidColorBrush(Colors.Blue),
            //    Stroke = new SolidColorBrush(Colors.Blue) { Opacity = 1 },
            //    StrokeThickness = 4,
            //    Opacity = 0.2
            //};
            //Canvas.SetLeft(selectionBox, selectionStartPoint.X);
            //Canvas.SetTop(selectionBox, selectionStartPoint.Y);
            //workspace.Children.Add(selectionBox);
            //workspace.CapturePointer(e.Pointer);
        }

        private void workspace_PointerMoved(object sender, PointerRoutedEventArgs e)
        {


            //else if (selecting)
            //{
            //    Point currPoint = e.GetCurrentPoint(workspace).Position;
            //    currPoint.X = (currPoint.X > workspace.Width) ? workspace.Width
            //                                                    : (currPoint.X < 0) ? 0 : currPoint.X;
            //    currPoint.Y = (currPoint.Y > workspace.Height) ? workspace.Height
            //                                                    : (currPoint.Y < 0) ? 0 : currPoint.Y;
            //    if (currPoint.X < selectionStartPoint.X)
            //    {
            //        Canvas.SetLeft(selectionBox, currPoint.X);
            //    }
            //    if (currPoint.Y < selectionStartPoint.Y)
            //    {
            //        Canvas.SetTop(selectionBox, currPoint.Y);
            //    }
            //    selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
            //    selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
            //}
        }

        private void workspace_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (linking)
            {
                linking = false;
                workspace.Children.Remove(currentLine);
                links.Remove(currentLine);
                currentLine = null;
            }

            //else if (selecting)
            //{
            //    Point currPoint = e.GetCurrentPoint(workspace).Position;
            //    selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
            //    selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
            //    workspace.Children.Remove(selectionBox);
            //    selecting = false;
            //    workspace.ReleasePointerCapture(e.Pointer);
            //}
        }
        
        public void DeleteModel(object sender, EventArgs e)
        {
            foreach (var link in findLinks((BaseModel)sender))
            {
                DeleteLink(link, e);
            }
            if(sender is BMM)
            {
                infobar.RemoveWarnings(sender, ((BMM)sender).warnings);
            }
            workspace.Children.Remove((BaseModel)sender);
            models.Remove((BaseModel)sender);
        }

        public void DeleteLink(object linkObj, EventArgs e)
        {
            var link = (Link)linkObj;
            workspace.Children.Remove(link);
            links.Remove(link);

            if (link.sourceModel is BMM && link.targetModel is BMM)
            {
                var source = (BMM)link.sourceModel;
                var target = (BMM)link.targetModel;
                source.validateRemovedLink(target.category, (findLinks(source)).FindAll(x => x.sourceModel is BMM && x.targetModel is BMM));
                target.validateRemovedLink(source.category, (findLinks(target)).FindAll(x => x.sourceModel is BMM && x.targetModel is BMM));
            }
        }

        private void ModelMoved(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var model = (BaseModel)sender;
            Point p = new Point(Canvas.GetLeft(model), Canvas.GetTop(model));
            model.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            workspace.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (p.X < 0)
            {
                workspace.Width = workspace.ActualWidth + Math.Abs(p.X);
                workspace.UpdateLayout();
                Canvas.SetLeft(model, 0);
            }
            else if (p.X + model.DesiredSize.Width > workspace.ActualWidth)
            {
                workspace.Width = p.X + model.DesiredSize.Width;
                workspaceScroll.ChangeView(p.X + model.DesiredSize.Width, null, null);
            }
            if (p.Y < 0)
            {
                workspace.Height = workspace.DesiredSize.Height + Math.Abs(p.Y);
                workspace.UpdateLayout();
                Canvas.SetTop(model, 0);
            }
            else if (p.Y + model.DesiredSize.Height > workspace.ActualHeight)
            {
                workspace.Height = p.Y + model.DesiredSize.Height;
                workspaceScroll.ChangeView(null, p.Y + model.DesiredSize.Height, null);
            }
        }
        private void ModelStoppedMoving(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var model = (BaseModel)sender;
            model.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double xBound = maxX();
            double yBound = maxY();
            workspace.Width = Math.Max(xBound, workspaceScroll.ActualWidth);
            workspace.Height = Math.Max(yBound, workspaceScroll.ActualHeight);
            workspaceScroll.ChangeView(Canvas.GetLeft(model), Canvas.GetTop(model), null);
        }

        private string serialize()
        {
            var root = new JsonObject();
            var modelArray = new JsonArray();
            var linkArray = new JsonArray();
            foreach (var child in workspace.Children)
            {
                if (child is BaseModel)
                {
                    modelArray.Add(((BaseModel)child).serialize());
                }
                else if (child is Link)
                {
                    linkArray.Add(((Link)child).serialize());
                }
            }
            root.Add("models", modelArray);
            root.Add("links", linkArray);
            Debug.WriteLine(root.Stringify());
            return root.Stringify();
        }

        private bool deserialize(string input)
        {
            JsonObject data;
            if (JsonObject.TryParse(input, out data) == false)
            {
                return false;
            }

            var modelArray = data.GetNamedArray("models", null);
            foreach(var entry in modelArray)
            {
                var value = entry.GetObject().GetNamedNumber("category", -1);
                if (value == -1)
                {
                    return false;
                }
                var type = (Category)value;
                BaseModel model =
                    (type == Category.NOTE) ? (BaseModel)NoteModel.deserialize(entry.GetObject()) :
                    (type == Category.BUSINESS_RULE) ? BusinessRuleModel.deserialize(entry.GetObject()) :
                    (type == Category.INFLUENCER) ? InfluencerModel.deserialize(entry.GetObject()) :
                    (type == Category.ASSESSMENT) ? AssessmentModel.deserialize(entry.GetObject()) :
                    BMM.deserialize(entry.GetObject());
                if (model != null)
                {
                    workspace.Children.Add(model);
                    models.Add(model);
                }
            }
            var linkArray = data.GetNamedArray("links", null);
            foreach (var entry in linkArray)
            {
                var link = Link.deserialize(entry.GetObject());
                if (link != null)
                {
                    workspace.Children.Add(link);
                    link.sourceModel.MovedEvent += link.sourceMoved;
                    link.targetModel.MovedEvent += link.targetMoved;
                }
            }
            return true;
        }

        private async void Save_Pressed(object sender, RoutedEventArgs e)
        {
            var fileSaver = new FileSavePicker();
            fileSaver.FileTypeChoices.Add("BPMM", new List<String>{ ".json" });
            var file = await fileSaver.PickSaveFileAsync();
            if (file != null)
            {
                
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    fileStream.Size = 0;
                    using (var outputStream = fileStream.GetOutputStreamAt(0))
                    {
                        using (var dataWriter = new DataWriter(outputStream))
                        {
                            dataWriter.WriteString(serialize());
                            await dataWriter.StoreAsync();
                            dataWriter.DetachStream();
                        }
                        await outputStream.FlushAsync();
                    }
                }
                Header = file.Path;
                MessageDialog infoPopup = new MessageDialog(String.Format("Saved to file {0}", file.Path), "Saved successfully!");
                await infoPopup.ShowAsync();
            }
        }

        private async void Load_Pressed(object sender, RoutedEventArgs e )
        {
            var fileOpener = new FileOpenPicker();
            fileOpener.FileTypeFilter.Add(".json");
            var file = await fileOpener.PickSingleFileAsync();
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (var inputStream = fileStream.GetInputStreamAt(0))
                    {
                        using (var streamReader = new StreamReader(inputStream.AsStreamForRead()))
                        {
                            string dataString = await streamReader.ReadToEndAsync();
                            if (deserialize(dataString) == false)
                            {
                                MessageDialog infoPopup = new MessageDialog(
                                        String.Format("Path: {0}. Not all entities could be loaded.", file.Path),
                                        "Failed to load complete diagram.");
                                await infoPopup.ShowAsync();
                            }
                            
                        }
                        inputStream.Dispose();
                    }
                }
                Header = file.Path;
            }
        }

        private async Task<string> ExportPNG()
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(workspace);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            var fileSaver = new FileSavePicker();
            fileSaver.FileTypeChoices.Add("Image", new List<String>{".png", ".jpeg", ".jpg", ".bmp", ".gif", ".tiff"});
            var file = await fileSaver.PickSaveFileAsync();
            if (file != null)
            { // see: http://loekvandenouweland.com/index.php/2013/12/save-xaml-as-png-in-a-windows-store-app/
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoderId =
                        (file.FileType == "jpeg" || file.FileType == "jpg") ? BitmapEncoder.JpegEncoderId :
                        (file.FileType == "bmp") ? BitmapEncoder.BmpEncoderId :
                        (file.FileType == "gif") ? BitmapEncoder.GifEncoderId :
                        (file.FileType == "tiff") ? BitmapEncoder.TiffEncoderId :
                        BitmapEncoder.PngEncoderId; // default
                    var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                         BitmapAlphaMode.Ignore,
                                         (uint)renderTargetBitmap.PixelWidth,
                                         (uint)renderTargetBitmap.PixelHeight, 96d, 96d,
                                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                return file.Path;
            }
            return null;
        }

        private async void Export_Pressed(object sender, RoutedEventArgs e)
        {
            var path = await ExportPNG();
            if (path != null)
            {
                MessageDialog infoPopup = new MessageDialog(String.Format("Saved to file {0}", path), "Saved successfully");
                await infoPopup.ShowAsync();
            }
        }

        private async void Clear_Pressed(object sender, RoutedEventArgs e)
        {
            await clearWorkspace();
        }

        private async Task<bool> clearWorkspace()
        {
            MessageDialog confirmDialog = new MessageDialog("", String.Format("Really clear workspace?"));
            confirmDialog.Commands.Add(new UICommand("Yes"));
            confirmDialog.Commands.Add(new UICommand("Cancel"));
            var result = await confirmDialog.ShowAsync();
            if (result.Label == "Yes")
            {
                Header = "unsaved diagram";
                workspace.Children.Clear();
                links.Clear();
                models.Clear();
                infobar.Warnings.Clear();
                BaseModel.resetIds();
                return true;
            }
            return false;
        }

        private List<Link> findLinks(BaseModel model)
        {
            var result = new List<Link>();
            foreach (var link in links)
            {
                if (link.sourceModel == model || link.targetModel == model)
                {
                    result.Add(link);
                }
            }
            return result;
        }
    }
}
