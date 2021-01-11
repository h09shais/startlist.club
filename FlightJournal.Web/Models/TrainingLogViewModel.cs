﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Apparently, we cannot carry the FlightContext around in EF, hence this Entity wrapper
    /// </summary>
    public class TrainingDataWrapper
    {
        //TODO: split this in a catalogue wrapper and a pilot/flight specific wrapper
        internal TrainingDataWrapper(FlightContext db, int pilotId, Flight flight, int trainingProgramId)
        {
            FlightId = flight.FlightId;
            // TrainingPrograms = db.TrainingPrograms;
            //TrainingLessons = db.TrainingLessons;
            //TrainingExercises = db.TrainingExercises;

            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId).OrderBy(x=>x.Date);
            FlightAnnotations = PilotFlights.SelectMany(x => db.TrainingFlightAnnotations.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y=>x.Date));
            TrainingProgram = db.TrainingPrograms.SingleOrDefault((x => x.Training2ProgramId == trainingProgramId)) ?? db.TrainingPrograms.First();
            TrainingPrograms = db.TrainingPrograms.Select(x => new TrainingProgramSelectorViewModel{Name = x.ShortName, Id = x.Training2ProgramId}).ToList();
        }

        public Training2Program TrainingProgram { get; }
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }

        // catalogue stuff
        //public IEnumerable<Training2Program> TrainingPrograms { get; }
        //public IEnumerable<Training.Training2Lesson> TrainingLessons { get; }
        //public IEnumerable<Training.Training2Exercise> TrainingExercises { get; }

        // flight specific data
        public Guid FlightId { get; }
        public IEnumerable<Flight> PilotFlights{ get; }
        public IEnumerable<AppliedExercise> AppliedExercises { get; } // across all PilotFlights
        public IEnumerable<TrainingFlightAnnotation> FlightAnnotations{ get; } // across all PilotFlights
    }

    public class TrainingProgramSelectorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TrainingProgramSelectorViewModel(){}

    }
    /// <summary>
    /// Viewmodel for an actual training flight
    /// </summary>
    public class FlightLogEntryViewModel
    {
        public DateTime Date { get; }
        public string Notes { get; }
        public string Maneuvers{ get; }
        public string Manouvres { get; }
        public string StartAnnotations{ get; }
        public string FlightAnnotations{ get; }
        public string ApproachAnnotations{ get; }
        public string LandingAnnotations{ get; }
        public IEnumerable<AppliedExerciseViewModel> ExercisesWithStatus { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="date"></param>
        /// <param name="exercises">Exercises applied in this flight</param> 
        /// <param name="annotations">Annotations for this flight</param>
        public FlightLogEntryViewModel(Flight flight, TrainingDataWrapper db, DateTime date)
        {
            Date = date;
            var annotationsForThisFlight = db.FlightAnnotations.Where(x => x.FlightId == flight.FlightId).ToList();
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flight.FlightId);
            Notes = string.Join("; ", annotationsForThisFlight.Select(x => x.Note));
            //TODO: localize / map to symbols
            Maneuvers = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.Maneuvers)));
            StartAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.StartAnnotation)));
            FlightAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.FlightAnnotation)));
            ApproachAnnotations = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.ApproachAnnotation)));
            LandingAnnotations= string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.LandingAnnotation)));
            ExercisesWithStatus = exercisesForThisFlight.Select(x => new AppliedExerciseViewModel(db,x));
        }

    }

    public class FlightPhaseAnnotationViewModel
    {
        public FlightPhaseAnnotation Id { get; }
        public string Name { get; }
        public string Icon { get; }

        public FlightPhaseAnnotationViewModel(FlightPhaseAnnotation id)
        {
            Id = id;
            switch (Id)
            {
                case FlightPhaseAnnotation.Ok:
                    Name = "&#x2713";
                    break;
                case FlightPhaseAnnotation.AlmostOk:
                    Name = "(&#x2713)";
                    break;
                case FlightPhaseAnnotation.Skull:
                    Name = "&#x2620";
                    break;
                default:
                    Name = Id.ToString();
                    break;
            }
        }
    }
    public class FlightManeuverViewModel
    {
        public FlightManeuver Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public FlightManeuverViewModel(FlightManeuver id)
        {
            Id = id;
            switch (id)
            {
                case FlightManeuver.Left90:
                case FlightManeuver.Right90:
                    Name = "90";
                    break;
                case FlightManeuver.Left180:
                case FlightManeuver.Right180:
                    Name = "180";
                    break;
                case FlightManeuver.Left360:
                case FlightManeuver.Right360:
                    Name = "360";
                    break;
                case FlightManeuver.FigureEight:
                    Name = "&infin;";
                    break;
                case FlightManeuver.Bank30:
                    Name = "&ang;30&deg;";
                    break;
                case FlightManeuver.Bank45:
                    Name = "&ang;45&deg;";
                    break;
                case FlightManeuver.Bank60:
                    Name = "&ang;60&deg;";
                    break;
                case FlightManeuver.AbortedStartLowAltitude:
                    Name = "&#x21B7 Afb start lavt";
                    break;
                case FlightManeuver.AbortedStartMediumAltitude:
                    Name = "&#x21B7 Afb start mellem";
                    break;
                case FlightManeuver.AbortedStartHighAltitude:
                    Name = "&#x21B7 Afb start højt";
                    break;
                case FlightManeuver.STurn:
                    Name = "&#x219D S-drej";
                    break;
                case FlightManeuver.LeftCircuit:
                    Name = "&#x21B0 Landingsrunde";
                    break;
                case FlightManeuver.RightCircuit:
                    Name = "&#x21B1 Landingsrunde";
                    break;
                default:
                    Name = id.ToString(); // TODO: localize
                    break;
            }
            switch (id)
            {
                case FlightManeuver.Left90:
                case FlightManeuver.Left180:
                case FlightManeuver.Left360:
                    Icon = "fa fa-undo";
                    break;
                case FlightManeuver.Right90:
                case FlightManeuver.Right180:
                case FlightManeuver.Right360:
                    Icon = " fa fa-repeat";
                    break;

            }
        }
    }



    public class WindSpeedViewModel
    {
        public int Value { get; }
        public string Text { get; }

        public WindSpeedViewModel(int speed)
        {
            Value = speed;
            Text = $"{speed}kn";
        }
    }

    public class WindDirectionViewModel
    {
        public int Value { get; }
        public string Text { get; }

        public WindDirectionViewModel(int direction)
        {
            Value = direction;
            Text = $"{direction}°";
        }
    }

    public class TrainingLogViewModel
    {
        public TrainingLogViewModel(DateTime date, string pilot, string backseatPilot, TrainingDataWrapper dbmodel)
        {
            Date = date;
            Pilot = pilot;
            BackseatPilot = backseatPilot;

            FlightLog = dbmodel.PilotFlights.Select(x=>new FlightLogEntryViewModel(x, dbmodel, date));

            // catalogue stuff
            TrainingProgram = new TrainingProgramViewModel(dbmodel.TrainingProgram, dbmodel);
            TrainingPrograms = dbmodel.TrainingPrograms;
            // replace this with manouvers
            Maneuvers = ((FlightManeuver[])Enum.GetValues(typeof(FlightManeuver))).Select(x=>new FlightManeuverViewModel(x));
            Annotations  = ((FlightPhaseAnnotation[])Enum.GetValues(typeof(FlightPhaseAnnotation))).Select(x=>new FlightPhaseAnnotationViewModel(x));

            var wd = new List<WindDirectionViewModel>();
            for (int v = 0; v < 360; v += 45)
                wd.Add(new WindDirectionViewModel(v ));
            WindDirections = wd;

            var ws = new List<WindSpeedViewModel>();
            for (int v = 0; v < 30; v += 5)
                ws.Add(new WindSpeedViewModel(v));
            WindSpeeds = ws;

        }
        public DateTime Date { get; }
        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<FlightLogEntryViewModel> FlightLog { get; }
        public TrainingProgramViewModel TrainingProgram;

        public IEnumerable<FlightManeuverViewModel> Maneuvers { get; }
        public IEnumerable<WindDirectionViewModel> WindDirections { get; }
        public IEnumerable<WindSpeedViewModel> WindSpeeds { get; }
        public IEnumerable<FlightPhaseAnnotationViewModel> Annotations{ get; }

        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }
    }

    public class TrainingProgramViewModel
    {
        public string Id { get; }

        public string Name { get; } 
        public IEnumerable<TrainingLessonWithOverallStatusViewModel> Lessons { get; }

        public TrainingProgramViewModel(Training2Program program, TrainingDataWrapper db)
        {
            Id = program.Training2ProgramId.ToString();
            Name = program.Name;
            Lessons = program.Lessons.Select(less => new TrainingLessonWithOverallStatusViewModel(less, db)).ToList();
        }
    }

    public class TrainingLessonWithOverallStatusViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public string Description { get; }
        public string Precondition { get; }

        public IEnumerable<TrainingExerciseWithOverallStatusViewModel> Exercises { get; }

        //TODO: for some reason, this calculation is broken. Numbers (and overall status) do not add up.
        public int ExercisesTotal { get; } 
        public int ExercisesCompleted { get; }
        public int ExercisesInProgress { get; }
        public int ExercisesNotStarted { get; }

        public string StatusSummary => $"{ExercisesNotStarted}/{ExercisesInProgress}/{ExercisesCompleted} ({ExercisesTotal})";
        public TrainingStatus Status { get; }

        public TrainingLessonWithOverallStatusViewModel(Training2Lesson lesson, TrainingDataWrapper db)
        {
            Id = lesson.Training2LessonId.ToString();
            Name = lesson.Name;
            Description = lesson.Purpose;
            Precondition = lesson.Precondition;
            Debug.WriteLine($"{Id} {Name}");
            Exercises = lesson.Exercises.Select(ex => new TrainingExerciseWithOverallStatusViewModel(ex, db)).ToList();
            ExercisesTotal = Exercises.Count();
            ExercisesCompleted = Exercises.Count(x => x.Status == TrainingStatus.Completed);
            ExercisesNotStarted = Exercises.Count(x => x.Status == TrainingStatus.NotStarted);
            ExercisesInProgress = ExercisesTotal - ExercisesCompleted - ExercisesNotStarted;
            Status = ExercisesTotal == ExercisesCompleted ? TrainingStatus.Completed 
                : ExercisesTotal == ExercisesNotStarted ? TrainingStatus.NotStarted 
                : TrainingStatus.Trained;
            Debug.WriteLine($"     {StatusSummary} -> {Status}");
        }
    }


    public class TrainingExerciseWithOverallStatusViewModel
    {
        public string Id { get; }

        public string Description { get; }
        public string LongDescription { get; }
        public TrainingStatus Status { get; }


        public bool IsBriefed => Status == TrainingStatus.Briefed 
                                 || Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;
        public bool IsTrained => Status == TrainingStatus.Trained
                                 || Status == TrainingStatus.Completed;
        public bool IsCompleted => Status == TrainingStatus.Completed;
        public bool BriefingOnlyRequired { get; } 

        public TrainingExerciseWithOverallStatusViewModel(Training2Exercise exercise, TrainingDataWrapper db)
        {
            Id = exercise.Training2ExerciseId.ToString();
            Description = exercise.Name;
            LongDescription = exercise.Note;
            BriefingOnlyRequired = exercise.IsBriefingOnly;

            if (true) // fake it for UI demo purposes
            {
                var toss = new Random().NextDouble();
                if (toss > 0.8)
                    Status = TrainingStatus.Completed;
                else if (toss > 0.6)
                    Status = TrainingStatus.Trained;
                else if (toss > 0.4)
                    Status = TrainingStatus.Briefed;
                else
                    Status = TrainingStatus.NotStarted;

                if(BriefingOnlyRequired)
                    Status = toss > 0.4 ? TrainingStatus.Briefed : TrainingStatus.NotStarted;

                Debug.WriteLine($"{Description}:{Status}  {IsBriefed}/{IsTrained}/{IsCompleted}");
            }
            else
            {
                var totalApplied = db.AppliedExercises.Where(x => x.Exercise == exercise).ToList();
                var isBriefed = totalApplied.Any(y => y.Action == ExerciseAction.Briefed);
                var isTrained = totalApplied.Any(y => y.Action == ExerciseAction.Trained);
                var isCompleted = totalApplied.Any(y => y.Action == ExerciseAction.Completed);

                Status =
                    isCompleted ? TrainingStatus.Completed :
                    isTrained ? TrainingStatus.Trained :
                    isBriefed ? TrainingStatus.Briefed :
                    TrainingStatus.NotStarted;
            }

            if (BriefingOnlyRequired && (Status == TrainingStatus.Briefed || Status == TrainingStatus.Trained))
                Status = TrainingStatus.Completed;

        }
    }


    public class AppliedExerciseViewModel
    {
        public string Description { get; }

        public ExerciseAction Action { get; set; }

        public AppliedExerciseViewModel(TrainingDataWrapper db, AppliedExercise appliedExercise)
        {
            Action = appliedExercise.Action;
            var program = db.TrainingProgram; // .TrainingPrograms.SingleOrDefault(x=>x == appliedExercise.Program);
            var lesson = program.Lessons.SingleOrDefault(x=>x == appliedExercise.Lesson);
            var exercise = lesson.Exercises.SingleOrDefault(x => x == appliedExercise.Exercise);
            Description = $"{program?.Name} {lesson?.Name} {exercise?.Name}";
        }
    }


    //TODO: add viewmodel for last 15 flights (
    //TODO: send back data to db (post): update db.AppliedExercises and db.TrainingFlightAnnotation (not created yet)
    //TODO: UI and models for SFIL quickselect

    public enum TrainingStatus
    {
        NotStarted,
        Briefed,
        Trained,
        Completed
    }
}