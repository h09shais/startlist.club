﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using FlightJournal.Web.Models.Training;
using FlightJournal.Web.Models.Training.Catalogue;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.Models
{
    /// <summary>
    /// Training relevant data for a specific flight (and pilot)
    /// 
    /// Apparently, we cannot carry the FlightContext around in EF, hence this Entity wrapper
    /// </summary>
    public class TrainingDataWrapper
    {
        internal TrainingDataWrapper(FlightContext db, int pilotId, Flight flight, int trainingProgramId)
        {
            FlightId = flight.FlightId;

            PilotFlights = db.Flights.Where(x => x.PilotId == pilotId).OrderBy(x => x.Date);
            FlightAnnotations = PilotFlights.SelectMany(x => db.TrainingFlightAnnotations.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            AppliedExercises = PilotFlights.SelectMany(x => db.AppliedExercises.Where(y => y.FlightId == x.FlightId).OrderBy(y => x.Date));
            TrainingProgram = db.TrainingPrograms.SingleOrDefault((x => x.Training2ProgramId == trainingProgramId)) ?? db.TrainingPrograms.First();
            TrainingPrograms = db.TrainingPrograms.Select(x => new TrainingProgramSelectorViewModel { Name = x.ShortName, Id = x.Training2ProgramId }).ToList();
            Manouvres = db.Manouvres.ToList();
            WindDirections = db.WindDirections.ToList();
            WindSpeeds = db.WindSpeeds.ToList();
           
        }

        /// <summary>
        /// The current (latest used) training program for the pilot
        /// </summary>
        public Training2Program TrainingProgram { get; }

        /// <summary>
        /// All available training programs
        /// </summary>
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }


        // flight specific data

        /// <summary>
        /// This flight
        /// </summary>
        public Guid FlightId { get; }

        /// <summary>
        /// All flights by the pilot
        /// </summary>
        public IEnumerable<Flight> PilotFlights { get; }
        /// <summary>
        /// Exercises flown across all flights by this pilot
        /// </summary>
        public IEnumerable<AppliedExercise> AppliedExercises { get; }
        /// <summary>
        /// Annotations (gradings/evaluations/comments) across all flights by this pilot
        /// </summary>
        public IEnumerable<TrainingFlightAnnotation> FlightAnnotations{ get; } // across all PilotFlights
        public IEnumerable<Manouvre> Manouvres { get; } // getting manouvres from the db
        public IEnumerable<WindSpeed> WindSpeeds {get;}
        public IEnumerable<WindDirection> WindDirections { get; }


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
            // one note per flight expected
            var annotationsForThisFlight = db.FlightAnnotations.Where(x => x.FlightId == flight.FlightId).ToList();
            Notes = string.Join("; ", annotationsForThisFlight.Select(x => x.Note));
            // multiple exercises possible per flight
            var exercisesForThisFlight = db.AppliedExercises.Where(x => x.FlightId == flight.FlightId);
            ExercisesWithStatus = exercisesForThisFlight.Select(x => new AppliedExerciseViewModel(db, x));

            //TODO: change each of these to a list
            //Manouvres = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.Maneuvers)));
            Manouvres = string.Join(",", annotationsForThisFlight.Select(x => string.Join(", ", x.Manouvres))); //-> load existing flight manouvres
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

    /// <summary>
    /// ViewModel for items used to describe a training flight.
    ///
    /// The actual input from the instructor ends up in a FlightLogEntryViewModel
    /// </summary>
    public class TrainingLogViewModel
    {
        public TrainingLogViewModel(DateTime date, string pilot, string backseatPilot, TrainingDataWrapper dbmodel)
        {
            Date = date;
            Pilot = pilot;
            BackseatPilot = backseatPilot;

            FlightLog = dbmodel.PilotFlights.Select(x=>new FlightLogEntryViewModel(x, dbmodel, x.Date));

            TrainingProgram = new TrainingProgramViewModel(dbmodel.TrainingProgram, dbmodel);
            TrainingPrograms = dbmodel.TrainingPrograms;
            // replace this with manouvers
            //Maneuvers = ((FlightManeuver[])Enum.GetValues(typeof(FlightManeuver))).Select(x=>new FlightManeuverViewModel(x));
            Manouvres = dbmodel.Manouvres;
/*            WindDirectionsDb = dbmodel.WindDirections;
            WindSpeedsDb = dbmodel.WindSpeeds;*/
            Annotations  = ((FlightPhaseAnnotation[])Enum.GetValues(typeof(FlightPhaseAnnotation))).Select(x=>new FlightPhaseAnnotationViewModel(x));


            //replace this with data from the DB
            var wds = new List<WindDirectionViewModel>();
            foreach(var wd in dbmodel.WindDirections){
                wds.Add(new WindDirectionViewModel(wd.WindDirectionItem));
            }
            WindDirections = wds;
/*            for (int v = 0; v < 360; v += 45)
                wd.Add(new WindDirectionViewModel(v ));
            WindDirections = wd;    */

            var wss = new List<WindSpeedViewModel>();
            foreach(var ws in dbmodel.WindSpeeds)
            {
                wss.Add(new WindSpeedViewModel(ws.WindSpeedItem));
            }
            WindSpeeds = wss;

            ThisFlight = new FlightLogEntryViewModel(dbmodel.PilotFlights.Single(x => x.FlightId == dbmodel.FlightId), dbmodel, date);

        }
        public DateTime Date { get; }
        public string Pilot { get; }
        public string BackseatPilot { get; }

        public IEnumerable<FlightLogEntryViewModel> FlightLog { get; } // previous flights
        public TrainingProgramViewModel TrainingProgram;

        // Selectable stuff
        public IEnumerable<Manouvre> Manouvres { get; }
        public IEnumerable<WindDirectionViewModel> WindDirections { get; }
        public IEnumerable<WindSpeedViewModel> WindSpeeds { get; }
        public IEnumerable<FlightPhaseAnnotationViewModel> Annotations{ get; }
        public IEnumerable<TrainingProgramSelectorViewModel> TrainingPrograms { get; }

        // data for this flight
        public FlightLogEntryViewModel ThisFlight { get; }

    }

    /// <summary>
    /// Viewmodel for a Training program (with lessons -> exercises).
    /// Used for building UI selections.
    /// </summary>
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

    /// <summary>
    /// ViewModel for a lesson (and its exercises) in the context of a pilot
    /// </summary>
    public class TrainingLessonWithOverallStatusViewModel
    {
        public string Id { get; }
        public string Name { get; }

        public string Description { get; }
        public string Precondition { get; }

        public IEnumerable<TrainingExerciseWithOverallStatusViewModel> Exercises { get; }

        public int ExercisesTotal { get; }
        /// <summary>
        /// Exercises completed by this pilot
        /// </summary>
        public int ExercisesCompleted { get; }
        /// <summary>
        /// Exercises in progress by this pilot
        /// </summary>
        public int ExercisesInProgress { get; }
        /// <summary>
        /// Exercises not yet started by this pilot
        /// </summary>
        public int ExercisesNotStarted { get; }

        public string StatusSummary => $"{ExercisesNotStarted}/{ExercisesInProgress}/{ExercisesCompleted} ({ExercisesTotal})";

        /// <summary>
        /// Overall status of the Lesson
        /// </summary>
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

    /// <summary>
    /// ViewModel for an exercise in the context of a pilot - used for overview
    /// </summary>
    public class TrainingExerciseWithOverallStatusViewModel
    {
        public string Id { get; }

        public string Description { get; }
        public string LongDescription { get; }
        /// <summary>
        /// Completion status by this pilot
        /// </summary>
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

            // TODO: use real data
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

    /// <summary>
    /// ViewModel for a flown exercise 
    /// </summary>
    public class AppliedExerciseViewModel
    {
        public string Description { get; }

        /// <summary>
        /// STatus of the flown exercise
        /// </summary>
        public ExerciseAction Action { get; set; }

        public AppliedExerciseViewModel(TrainingDataWrapper db, AppliedExercise appliedExercise)
        {
            Action = appliedExercise.Action;
            var program = db.TrainingProgram;
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