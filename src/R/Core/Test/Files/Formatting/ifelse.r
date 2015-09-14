while (guess != num) {
  guess <- readinteger()
  if (guess == num) {
    cat("Congratulations,", num, "is right.\n")
  }
  else if (guess < num) {
    cat("It's bigger!\n'")
  }
  else 
  if (guess > num) {
    cat("It's smaller!\n")
  }
}
