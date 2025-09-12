# CadEye

CadEye is a CAD history tracking tool designed for Zwcad.  
This project is built with .NET Framework and allows users to manage and review their CAD activity records efficiently.


⚠ Note: This project was developed in approximately 2 weeks, with about 1 week of debugging. 
There are still known bugs, so please use it for testing purposes only.

---

## Libraries Used

This project uses the following open-source libraries, all licensed under MIT License:

- [PDFsharp](https://github.com/empira/PDFsharp)
- [LiteDB](https://github.com/mbdavid/LiteDB)
- [PdfiumViewer](https://github.com/pvginkel/PdfiumViewer)

---

## tree
C:.
│  App.xaml
│  App.xaml.cs
│  MainWindow.xaml
│  MainWindow.xaml.cs
│
├─Behavior
│      BindableSelectedItemBehaviorDataGrid.cs
│      BindableSelectedItemBehaviorTree.cs
│ 
├─Commands
│  │  FolderSelect.cs
│  │  NewWindowHost.cs
│  │  ViewSwitch.cs
│  │
│  └─Helpers
│          AsyncCommand.cs
│          AsyncCommandT.cs
│          RelayCommand.cs
│          RetryHelper.cs
│
├─Models
│  │  TreeNode.cs
│  │
│  └─Entity
│          ChildFile.cs
│          EventEntry.cs
│          FileViewEntry.cs
│          ImageEntry.cs
│          RefEntry.cs
├─Services
│  ├─FileCheck
│  │      FileCheckService.cs
│  │      IFileCheckService.cs
│  │
│  ├─FileSystem
│  │      FileSystem.cs
│  │      IFileSystem.cs
│  │
│  ├─FileWatcher
│  │  ├─ProjectFolder
│  │  │      IProjectFolderWatcherService.cs
│  │  │      ProjectFolderWatcherService.cs
│  │  │
│  │  ├─Repository
│  │  │      IRepositoryWatcherService.cs
│  │  │      RepositoryWatcherService.cs
│  │  │
│  │  └─RepositoryPdf
│  │          IRepositoryPdfWatcherService.cs
│  │          RepositoryPdfWatcherService.cs
│  │
│  ├─FolderService
│  │      FolderService.cs
│  │      iFolderService.cs
│  │
│  ├─Mongo
│  │  ├─Interfaces
│  │  │      IChildFileService.cs
│  │  │      IEventEntryService.cs
│  │  │      IImageEntryService.cs
│  │  │      IRefEntryService.cs
│  │  │
│  │  └─Services
│  │          MongoDBChildFile.cs
│  │          MongoDBEventEntry.cs
│  │          MongoDBImageEntry.cs
│  │          MongoDBRefEntry.cs
│  │
│  ├─PDF
│  │      IPdfService.cs
│  │      PdfService.cs
│  │
│  ├─WindowService
│  │      IWindowsService.cs
│  │      WindowService.cs
│  │
│  └─Zwcad
│          IZwcadService.cs
│          ZwcadService.cs
│
├─Style
│      Styles.xaml
│
├─ViewModels
│  │  HomeViewModel.cs
│  │  InformationViewModel.cs
│  │  MainViewModel.cs
│  │
│  └─Messages
│          SelectedTreeNodeMessage.cs
│          SendByteMessage.cs
│          SendCompareByteMessage.cs
│          SendStatusMessage.cs
│
└─Views
        HomeView.xaml
        HomeView.xaml.cs
        InformationView.xaml
        InformationView.xaml.cs

## License

Copyright © GitHub user 'qqqqaqaqaqq'.  
This project and the libraries it uses are licensed under the MIT License.  
You are free to use, modify, and distribute it in accordance with the license terms.

