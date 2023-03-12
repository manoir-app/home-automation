using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Common.Model
{
    public class RecipeCuisine
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class RecipeCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Recipe
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<string> Images { get; set; } = new List<string>();
        public List<RecipeVideo> Videos { get; set; }

        public string SourceKind { get; set; }
        public string SourceUrl { get; set; }

        public string Description { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();

        public string Servings { get; set; }
        public string RecipeCuisineId { get; set; }
        public string RecipeCategoryId { get; set; }

        public List<string> SuitableForDiets { get; set; } = new List<string>();

        public List<string> PossibleAllergies { get; set; } = new List<string>();

        public List<RecipeInstructions> Instructions { get; set; } = new List<RecipeInstructions>();

        public List<string> ToolsNeeded { get; set; }

        public RecipeNutritionInfo NutritionInfo { get; set; }
    }

    public class RecipeNutritionInfo
    {
        public decimal? Calories { get; set; }
        public decimal? Sugar { get; set; }
        public decimal? Sodium
        { get; set; }
    }
    public class RecipeInstructions
    {
        public string Name { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

        public TimeSpan? PreparationTime { get; set; }
        public TimeSpan? CookingTime { get; set; }
        public TimeSpan? WaitingTime { get; set; }

        public List<RecipeStep> Steps { get; set; }
    }

    public class RecipeStep
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HelpUrl { get; set; }
        public string AssociatedVideo { get; set; }
        public TimeSpan? Timecode { get; set; }
    }

    public class RecipeVideo
    {
        public string VideoType { get; set; }
        public string VideoUrl { get; set; }
    }

    public class RecipeIngredient
    {
        public string Name { get; set; }
        public string ProductId { get; set; }
        public string UnitId { get; set; }
        public decimal UnitCount { get; set; }
    }

}
