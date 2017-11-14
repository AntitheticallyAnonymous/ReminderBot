using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReminderBot;
using System.Reflection;
using Rhino.Mocks;
using FluentAssertions;

namespace ReminderBotTests
{
    [TestClass]
    public class CommandHandlerTests
    {

        [TestCleanup]
        public void TearDown()
        {
            TimeProvider.ResetToDefault();
        }

        #region ParseRepeat Tests
        #region Valid Cases
        [TestMethod]
        public void ParseRepeat_NoRepeat()
        {
            string[] args = new string[] { ".r", ".r" };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int) accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 0);            
        }

        [TestMethod]
        public void ParseRepeat_RepeatZero()
        {
            string command = ".r0";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 0);
        }

        [TestMethod]
        public void ParseRepeat_RepeatOnce()
        {
            string command = ".r1";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 1);
        }

        [TestMethod]
        public void ParseRepeat_RepeatTwice()
        {
            string command = ".r2";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 2);
        }

        [TestMethod]
        public void ParseRepeat_Repeat614364()
        {
            string command = ".r614364";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 614364);
        }

        [TestMethod]
        public void ParseRepeat_RepeatMax()
        {
            string command = ".r" + int.MaxValue.ToString();
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, int.MaxValue);
        }

        [TestMethod]
        public void ParseRepeat_RepeatForever()
        {
            string command = ".rr" ;
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -1);
        }

        [TestMethod]
        public void ParseRepeat_NotDefaultPrefix()
        {
            string command = "!remind24";
            string prefix = "!remind";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, 24);
        }

        #endregion

        #region Invalid Cases
        [TestMethod]
        public void ParseRepeat_RepeatMin()
        {
            string command = ".r" + int.MinValue;
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }

        [TestMethod]
        public void ParseRepeat_RepeatNegativeOne()
        {
            string command = ".r-1";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }        

        [TestMethod]
        public void ParseRepeat_MismatchedPrefix()
        {
            string command = ".r";
            string prefix = "r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }

        [TestMethod]
        public void ParseRepeat_RepeatDecimal()
        {
            string command = ".r1.5";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }

        [TestMethod]
        public void ParseRepeat_DoubleR()
        {
            string command = ".rrr";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }

        [TestMethod]
        public void ParseRepeat_CommandHasSpace()
        {
            string command = ".r r";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseRepeat", args);
            Assert.AreEqual(result, -2);
        }

        [TestMethod]
        public void ParseRepeat_EmptyCommand()
        {
            string command = "";
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Value cannot be null.\r\nParameter name: command");
        }

        [TestMethod]
        public void ParseRepeat_NullCommand()
        {
            string command = null;
            string prefix = ".r";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);
            
            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Value cannot be null.\r\nParameter name: command");
        }

        [TestMethod]
        public void ParseRepeat_EmptyPrefix()
        {
            string command = "r";
            string prefix = "";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_EmptyPrefixEmptyCommand()
        {
            string command = "";
            string prefix = "";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_EmptyPrefixRepeatOnce()
        {
            string command = "1";
            string prefix = "";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_EmptyPrefixRepeatForever()
        {
            string command = "r";
            string prefix = "";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_EmptyPrefixNullCommand()
        {
            string command = null;
            string prefix = "";
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_NullPrefixEmptyCommand()
        {
            string command = "";
            string prefix = null;
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_NullPrefixRepeatOnce()
        {
            string command = "1";
            string prefix = null;
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_NullPrefixRepeatForever()
        {
            string command = "r";
            string prefix = null;
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }

        [TestMethod]
        public void ParseRepeat_NullPrefixNullCommand()
        {
            string command = null;
            string prefix = null;
            string[] args = new string[] { command, prefix };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            Action action = () => accessor.Invoke("ParseRepeat", args);

            action
                .ShouldThrow<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>()
                .WithInnerMessage("Check credentials file.\r\nParameter name: prefix");
        }
        #endregion
        #endregion

        #region ParseCommand Tests
        #endregion

        #region ParseWhen Tests
        #region Valid Cases
        

        [TestMethod]
        public void ParseWhen_One()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;            
            
            string[] args = new string[] { ".r", "1" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);           
            Assert.AreEqual(new DateTimeOffset(2017, 7, 1, 12, 31, 0, TimeSpan.Zero), parameters[2]);
            Assert.AreEqual(1, parameters[3]);
            
            Assert.AreEqual(1, result);        
        }

        [TestMethod]
        public void ParseWhen_Two()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "2" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            Assert.AreEqual(new DateTimeOffset(2017, 7, 1, 12, 32, 0, TimeSpan.Zero), parameters[2]);
            Assert.AreEqual(2, parameters[3]);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ParseWhen_Thirty()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "30" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            Assert.AreEqual(new DateTimeOffset(2017, 7, 1, 13, 0, 0, TimeSpan.Zero), parameters[2]);
            Assert.AreEqual(30, parameters[3]);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ParseWhen_DateTimeDecimalMMDDYYYY()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "12.31.2017" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            Assert.AreEqual(new DateTimeOffset(2017, 12, 31, 0, 0, 0, TimeSpan.Zero), parameters[2]);
            Assert.AreEqual(1440, parameters[3]);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void ParseWhen_DateTimeDecimalYYYYMMDD()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "2017.12.31" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            Assert.AreEqual(new DateTimeOffset(2017, 12, 31, 0, 0, 0, TimeSpan.Zero), parameters[2]);
            Assert.AreEqual(1440, parameters[3]);

            Assert.AreEqual(1, result);
        }
        #endregion

        #region Invalid Cases

        [TestMethod]
        public void ParseWhen_Zero()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "0" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ParseWhen_NegativeOne()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "-1" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ParseWhen_NegativeTwo()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "-2" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void ParseWhen_NoArg()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void ParseWhen_SingleArg()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void ParseWhen_EmptySecondArg()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void ParseWhen_SpaceSecondArg()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", " " };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void ParseWhen_Decimal()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", ".2" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void ParseWhen_DecimalDayMonthPast()
        {
            //Note: Decimals may be used to represent date depending on style
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "1.2" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);
            
            Assert.AreNotEqual(1, result);            
        }

        [TestMethod]
        public void ParseWhen_DateTimeDecimalDDMMYYYY()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "31.12.2017" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }


        [TestMethod]
        public void ParseWhen_DateTimeDecimalYYYYDDMM()
        {
            DateTimeOffset timestamp = new DateTimeOffset(2017, 7, 1, 12, 30, 0, TimeSpan.Zero);

            TimeProvider fakeTimeProvider = MockRepository.GenerateStub<TimeProvider>();
            fakeTimeProvider.Expect(x => x.UtcNow).Return(timestamp);
            TimeProvider.Current = fakeTimeProvider;

            string[] args = new string[] { ".r", "2017.31.12" };
            object[] parameters = new object[] { args, timestamp, null, null };
            CommandHandler ch = new CommandHandler(null, null);
            PrivateObject accessor = new PrivateObject(ch);
            int result = (int)accessor.Invoke("ParseWhen", parameters);

            Assert.AreEqual(-1, result);
        }

        #endregion
        #endregion

        #region ParseReminderMessage Tests
        #endregion
    }
}
;