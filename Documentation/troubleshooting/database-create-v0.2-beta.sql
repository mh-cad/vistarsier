USE [VT]
GO
/****** Object:  Table [dbo].[Attempts]    Script Date: 11/10/2019 11:02:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Attempts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CurrentAccession] [nvarchar](max) NULL,
	[SourceAet] [nvarchar](max) NULL,
	[DestinationAet] [nvarchar](max) NULL,
	[PatientId] [nvarchar](max) NULL,
	[PatientFullName] [nvarchar](max) NULL,
	[PatientBirthDate] [nvarchar](max) NULL,
	[CurrentSeriesUID] [nvarchar](max) NULL,
	[PriorAccession] [nvarchar](max) NULL,
	[PriorSeriesUID] [nvarchar](max) NULL,
	[ReferenceSeries] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[JobId] [bigint] NULL,
	[Comment] [nvarchar](max) NULL,
	[DbExt] [nvarchar](max) NULL,
	[Method] [int] NOT NULL,
	[CustomRecipe] [nvarchar](max) NULL,
 CONSTRAINT [PK_Attempts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jobs]    Script Date: 11/10/2019 11:02:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jobs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Status] [nvarchar](max) NULL,
	[Start] [datetime2](7) NOT NULL,
	[End] [datetime2](7) NOT NULL,
	[RecipeString] [nvarchar](max) NULL,
	[DbExt] [nvarchar](max) NULL,
	[AttemptId] [bigint] NULL,
	[RecipeId] [bigint] NULL,
 CONSTRAINT [PK_Jobs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StoredRecipes]    Script Date: 11/10/2019 11:02:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StoredRecipes](
	[Id] [bigint] NOT NULL,
	[UserEditable] [bit] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[RecipeString] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_StoredRecipes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
