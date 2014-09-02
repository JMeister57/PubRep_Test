using BPMM_App.Common;
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

namespace BPMM_App
{

    public sealed partial class MainPage : Page
    {      
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public static List<BaseControl> controls = new List<BaseControl>();

        bool associating;
        private AssociationControl currentLine;
        private BaseControl sourceControl;
        
        private bool selecting;
        private Point selectionStartPoint;
        private Rectangle selectionBox;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            associating = false;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // restore session data
            if (e.PageState != null && e.PageState.ContainsKey("greetingOutputText"))
            {

            }
            // restore app data
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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
                if (item is BPMM_Object.Type == false)
                {
                    NoteControl note = addNote();
                    note.AssociationStartEvent += OnAssociationStart;
                    note.AssociationEndEvent += OnAssociationRequest;
                    note.DeleteEvent += DeleteControl;
                    Point point = e.GetPosition(workspace);
                    Canvas.SetLeft(note, point.X);
                    Canvas.SetTop(note, point.Y);
                    workspace.Children.Add(note);
                    return;
                }
                BPMM_Object.Type type = (BPMM_Object.Type)item;
                BPMM_Object obj;
                String title;
                switch (type)
                {
                    case BPMM_Object.Type.VISION:
                        obj = (BPMM_Object) new Vision();
                        title = "Vision";
                        break;
                    case BPMM_Object.Type.GOAL:
                        obj = (BPMM_Object) new Goal();
                        title = "Goal";
                        break;
                    case BPMM_Object.Type.OBJECTIVE:
                        obj = (BPMM_Object) new Objective();
                        title = "Objective";
                        break;
                    case BPMM_Object.Type.MISSION:
                        obj = (BPMM_Object) new Mission();
                        title = "Mission";
                        break;
                    case BPMM_Object.Type.STRATEGY:
                        obj = (BPMM_Object) new Strategy();
                        title = "Strategy";
                        break;
                    case BPMM_Object.Type.TACTIC:
                        obj = (BPMM_Object) new Tactic();
                        title = "Tactic";
                        break;
                    case BPMM_Object.Type.BUSINESS_POLICY:
                        obj = (BPMM_Object) new BusinessPolicy();
                        title = "Policy";
                        break;
                    case BPMM_Object.Type.BUSINESS_RULE:
                        obj = (BPMM_Object) new BusinessRule();
                        title = "Rule";
                        break;
                    case BPMM_Object.Type.INFLUENCER:
                        obj = (BPMM_Object)new Influencer();
                        title = "Influencer";
                        break;
                    case BPMM_Object.Type.ASSESSMENT:
                        obj = (BPMM_Object) new Assessment();
                        title = "Assessment";
                        break;
                    default:
                        return;
                }
                BPMMControl control = addBPMMControl(obj);
                control.viewModel.Title = title;
                control.AssociationStartEvent += OnAssociationStart;
                control.AssociationEndEvent += OnAssociationRequest;
                control.DeleteEvent += DeleteControl;
                Point pos = e.GetPosition(workspace);
                Canvas.SetLeft(control, pos.X);
                Canvas.SetTop(control, pos.Y);
                workspace.Children.Add(control);                
            }
        }


        private BPMMControl addBPMMControl(BPMM_Object obj)
        {
            var control = new BPMMControl(obj);
            controls.Add(control);
            return control;
        }

        private NoteControl addNote()
        {
            var note = new NoteControl();
            controls.Add(note);
            return note;
        }

        private void delete(BaseControl control)
        {
            for (int i = 0; i < controls.Count; ++i)
            {
                if (controls[i] == control)
                {
                    controls.RemoveAt(i);
                    break;
                }
            }
        }

        public static BaseControl getControl(int id)
        {
            foreach (var control in controls)
            {
                if (control.id == id)
                {
                    return control;
                }
            }
            return null;
        }

        private void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items[0].Equals(visionIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.VISION); }
            else if (e.Items[0].Equals(goalIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.GOAL); }
            else if (e.Items[0].Equals(objectiveIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.OBJECTIVE); }
            else if (e.Items[0].Equals(missionIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.MISSION); }
            else if (e.Items[0].Equals(strategyIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.STRATEGY); }
            else if (e.Items[0].Equals(tacticIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.TACTIC); }
            else if (e.Items[0].Equals(policyIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.BUSINESS_POLICY); }
            else if (e.Items[0].Equals(ruleIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.BUSINESS_RULE); }
            else if (e.Items[0].Equals(influencerIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.INFLUENCER); }
            else if (e.Items[0].Equals(assessmentIcon)) { e.Data.Properties.Add("Item", BPMM_Object.Type.ASSESSMENT); }
            else if (e.Items[0].Equals(note)) { e.Data.Properties.Add("Item", "Note"); }
        }
        #region association drawing
        public void OnAssociationStart(object sender, PointerRoutedEventArgs e)
        {
            sourceControl = (BaseControl)sender;
            Point p = new Point(Canvas.GetLeft(sourceControl), Canvas.GetTop(sourceControl));
            currentLine = new AssociationControl(sourceControl, p, e.GetCurrentPoint((UIElement)sender).Position);
            sourceControl.MovedEvent += currentLine.sourceMoved;
            sourceControl.DeleteEvent += currentLine.Delete;
            currentLine.DeleteEvent += DeleteAssociation;
            workspace.Children.Add(currentLine);
            associating = true;
        }

        public void OnAssociationRequest(object sender, EventArgs e)
        {
            if (currentLine == null)
            { // case: simple clickon control
                return;
            }

            BaseControl target = (BaseControl)sender;
            Point p = new Point(Canvas.GetLeft(target), Canvas.GetTop(target));

            if (currentLine.viewModel.Points[0].Equals(p))
            { // case: association to itself
                Debug.WriteLine("Cannot pull association to itself.");
                workspace.Children.Remove(currentLine);
                return;
            }
            bool linked = (sourceControl is BPMMControl)? ((BPMMControl)sourceControl).LinkWith(target) : ((NoteControl)sourceControl).LinkWith(target);
            if (linked)
            { // case: allowed association
                currentLine.updateEndPoint(target, p);
                target.MovedEvent += currentLine.targetMoved;
                target.DeleteEvent += currentLine.Delete;
                sourceControl = null;
            }
            else
            { // case: misfitting BPMM objects
                Debug.WriteLine("these two objects cannot be linked");
                workspace.Children.Remove(currentLine);
            }
            sourceControl = null;
            currentLine = null;
            associating = false;
        }
        #endregion

        private void workspace_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (associating)
            {
                associating = false;
                workspace.Children.Remove(currentLine);
                currentLine = null;
                return;
            } 
            selecting = true;
            selectionStartPoint = e.GetCurrentPoint(workspace).Position;
            selectionBox = new Rectangle()
            {
                Width = Math.Abs(selectionStartPoint.X - selectionStartPoint.X),
                Height = Math.Abs(selectionStartPoint.Y - selectionStartPoint.Y),
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.Blue) { Opacity = 1 },
                StrokeThickness = 4,
                Opacity = 0.2
            };
            Canvas.SetLeft(selectionBox, selectionStartPoint.X);
            Canvas.SetTop(selectionBox, selectionStartPoint.Y);
            workspace.Children.Add(selectionBox);
            workspace.CapturePointer(e.Pointer);
        }

        private void workspace_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (associating)
            {
                Point target = e.GetCurrentPoint((UIElement)sender).Position;
                target.X -= 5;
                target.Y -= 5;
                currentLine.updateEndPoint(null, target);
            }
            else if (selecting)
            {
                Point currPoint = e.GetCurrentPoint(workspace).Position;
                currPoint.X = (currPoint.X > workspace.Width) ? workspace.Width
                                                                : (currPoint.X < 0) ? 0 : currPoint.X;
                currPoint.Y = (currPoint.Y > workspace.Height) ? workspace.Height
                                                                : (currPoint.Y < 0) ? 0 : currPoint.Y;
                if (currPoint.X < selectionStartPoint.X)
                {
                    Canvas.SetLeft(selectionBox, currPoint.X);
                }
                if (currPoint.Y < selectionStartPoint.Y)
                {
                    Canvas.SetTop(selectionBox, currPoint.Y);
                }
                selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
                selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
            }
        }

        private void workspace_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (associating)
            {
                associating = false;
                workspace.Children.Remove(currentLine);
                currentLine = null;
            }

            else if (selecting)
            {
                Point currPoint = e.GetCurrentPoint(workspace).Position;
                selectionBox.Width = Math.Abs(currPoint.X - selectionStartPoint.X);
                selectionBox.Height = Math.Abs(currPoint.Y - selectionStartPoint.Y);
                workspace.Children.Remove(selectionBox);
                selecting = false;
                workspace.ReleasePointerCapture(e.Pointer);
            }
        }
        
        public void DeleteControl(object sender, EventArgs e)
        {
            workspace.Children.Remove((BaseControl)sender);
            delete((BaseControl)sender);
        }

        public void DeleteAssociation(object sender, EventArgs e)
        {
            workspace.Children.Remove((AssociationControl)sender);
        }

        private string serialize()
        {
            var root = new JsonObject();
            var bpmmArray = new JsonArray();
            var noteArray = new JsonArray();
            var associationArray = new JsonArray();
            foreach (var child in workspace.Children)
            {
                if (child is BPMMControl)
                {
                    bpmmArray.Add(((BPMMControl)child).serialize());
                }
                else if (child is NoteControl)
                {
                    noteArray.Add(((NoteControl)child).serialize());
                }
                else if (child is AssociationControl)
                {
                    associationArray.Add(((AssociationControl)child).serialize());
                }
            }
            root.Add("bpmms", bpmmArray);
            root.Add("notes", noteArray);
            root.Add("associations", associationArray);
            Debug.WriteLine(root.Stringify());
            return root.Stringify();
        }

        private bool deserialize(string input)
        {
            Debug.WriteLine(input);
            JsonObject data;
            if (JsonObject.TryParse(input, out data) == false)
            {
                return false;
            }

            var bpmmArray = data.GetNamedArray("bpmms", null);
            foreach(var entry in bpmmArray)
            {
                var control = BPMMControl.deserialize(entry.GetObject());
                if (control != null)
                {
                    workspace.Children.Add(control);
                    controls.Add(control);
                }
            }
            var noteArray = data.GetNamedArray("notes", null);
            foreach (var entry in noteArray)
            {
                var note = NoteControl.deserialize(entry.GetObject());
                if (note != null)
                {
                    workspace.Children.Add(note);
                    controls.Add(note);
                }
            }
            var associationArray = data.GetNamedArray("associations", null);
            foreach (var entry in associationArray)
            {
                var association = AssociationControl.deserialize(entry.GetObject());
                if (association != null)
                {
                    workspace.Children.Add(association);
                    var source = getControl(association.sourceId);
                    var target = getControl(association.targetId);
                    if (source == null || target == null)
                    {
                        return false;
                    }
                    source.MovedEvent += association.sourceMoved;
                    target.MovedEvent += association.targetMoved;
                    source.DeleteEvent += association.Delete;
                    target.DeleteEvent += association.Delete;
                }
            }
            return true;
        }

        private async void Save_Pressed(object sender, PointerRoutedEventArgs e)
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
                MessageDialog infoPopup = new MessageDialog(String.Format("Saved to file {0}", file.Path), "Saved successfully!");
                await infoPopup.ShowAsync();
            }
        }

        private async void Load_Pressed(object sender, PointerRoutedEventArgs e )
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
                                MessageDialog infoPopup = new MessageDialog(String.Format("Path: {0}. Not all entities could be loaded.", file.Path), "Failed to load complete diagram.");
                                await infoPopup.ShowAsync();
                            }
                            
                        }
                        inputStream.Dispose();
                    }
                }
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
                    var encoderId = (file.FileType == "jpeg" || file.FileType == "jpg") ? BitmapEncoder.JpegEncoderId
                        : (file.FileType == "bmp") ? BitmapEncoder.BmpEncoderId
                        : (file.FileType == "gif") ? BitmapEncoder.GifEncoderId
                        : (file.FileType == "tiff") ? BitmapEncoder.TiffEncoderId
                        : BitmapEncoder.PngEncoderId; // default
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

        private async void Export_Pressed(object sender, PointerRoutedEventArgs e)
        {
            var path = await ExportPNG();
            if (path != null)
            {
                MessageDialog infoPopup = new MessageDialog(String.Format("Saved to file {0}", path), "Saved successfully");
                await infoPopup.ShowAsync();
            }
        }

        private async void Clear_Pressed(object sender, PointerRoutedEventArgs e)
        {
            MessageDialog confirmDialog = new MessageDialog("", String.Format("Really clear workspace?"));
            confirmDialog.Commands.Add(new UICommand("Yes"));
            confirmDialog.Commands.Add(new UICommand("Cancel"));
            var result = await confirmDialog.ShowAsync();
            if (result.Label == "Yes")
            {
                workspace.Children.Clear();
            }
        }
    }
}
